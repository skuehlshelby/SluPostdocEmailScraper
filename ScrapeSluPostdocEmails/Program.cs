using System;
using System.Net.Http;
using HtmlAgilityPack;
using CommandDotNet;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace SluEmailScraper
{
    public class Program
    {
        private readonly Settings settings = Settings.Load();

        public static void Main(string[] args)
        {
            try
            {
                AppRunner<Program> runner = new AppRunner<Program>();
                runner.Run(args);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                Console.Error.WriteLine(e.StackTrace);
                Environment.ExitCode = 1;
            }
        }

        [DefaultCommand()]
        [Command("update-cache-then-parse-from-cache")]
        public void UpdateCacheThenParseFromCache()
        {
            ScrapeFromSluWebpageAndSaveToCache();
            ParseFromCache();
            Console.Write("Press any key to exit...");
            Console.ReadKey();
        }

        [Command("update-cache", Description = CommandDescriptions.UPDATE_CACHE, ExtendedHelpText = CommandDescriptions.UPDATE_CACHE_EXTENDED)]
        public void ScrapeFromSluWebpageAndSaveToCache()
        {
            foreach (var campus in settings.TargetCampuses)
            {
                foreach (var department in campus.Departments)
                {
                    Console.WriteLine($"Querying SLU for '{department}' employee data...");

                    string query = settings.SluSearchUrl.Replace("DEPARTMENTNAME", Uri.EscapeDataString(department));
                    string webpage = LoadWebpage(query);

                    Console.WriteLine("Data received.");

                    string filename = department + ".html";
                    SaveToCacheAsHtml(filename, webpage);

                    Console.WriteLine($"Data saved to '{Path.Combine(settings.CachedSearchResultsDirectory, filename)}'.");
                    Console.WriteLine("Waiting briefly between queries...");

                    Thread.Sleep(2000);
                }
            }
            
            Console.WriteLine("Done.");

            settings.Save();
        }

        private string LoadWebpage(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, url);
                HttpResponseMessage response = client.Send(message);
                response.EnsureSuccessStatusCode();

                using (StreamReader reader = new StreamReader(response.Content.ReadAsStream()))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private void SaveToCacheAsHtml(string fileName, string content)
        {
            if (!Directory.Exists(settings.CachedSearchResultsDirectory))
            {
                Directory.CreateDirectory(settings.CachedSearchResultsDirectory);
            }

            using (StreamWriter writer = File.CreateText(Path.Combine(settings.CachedSearchResultsDirectory, fileName)))
            {
                writer.Write(content);
            }
        }

        [Command("parse-from-cache", Description = CommandDescriptions.PARSE_FROM_CACHE, ExtendedHelpText = CommandDescriptions.PARSE_FROM_CACHE_EXTENDED)]
        public void ParseFromCache()
        {
            DirectoryInfo cache = new DirectoryInfo(settings.CachedSearchResultsDirectory);

            if (cache.Exists)
            {
                ISet<Person> people = new HashSet<Person>();

                foreach (var campus in settings.TargetCampuses)
                {
                    foreach (var department in campus.Departments)
                    {
                        FileInfo cachedHtml = new FileInfo(Path.Combine(cache.FullName, department + ".html"));

                        if (cachedHtml.Exists)
                        {
                            HtmlDocument workingHtml = new HtmlDocument();
                            workingHtml.Load(cachedHtml.OpenRead());

                            HtmlNodeCollection searchResults = workingHtml.DocumentNode.SelectNodes(settings.SearchResultXPath);

                            foreach (var searchResult in searchResults)
                            {
                                Person person = ProcessSearchResult(searchResult, campus.Name, department);

                                if (settings.TargetJobs.Contains(person.Job))
                                {
                                    people.Add(person);
                                }
                            }
                        }
                    }
                }
                
                SavePeopleToOutputDirectoryAsCsv("employee-email-data", people);

                settings.Save();
            }
            else
            {
                Console.WriteLine(settings.CachedSearchResultsDirectory + " does not exist.");
                Console.WriteLine("No results parsed. Please run 'update-cache' first.");
            }
        }

        private Person ProcessSearchResult(HtmlNode result, string campus, string department)
        {
            string name = result.SelectSingleNode(settings.NameXPath)?.InnerText;
            string email = result.SelectSingleNode(settings.EmailXPath)?.InnerText;
            string job = result.SelectSingleNode(settings.JobXPath)?.InnerText.Replace(settings.JobKeyPhrase, string.Empty).Trim();
            
            return new Person(name, job, email, campus, department);
        }

        private void SavePeopleToOutputDirectoryAsCsv(string fileName, ICollection<Person> people)
        {
            if (people.Any())
            {
                DirectoryInfo output = Directory.CreateDirectory(settings.OutputDirectory);

                using (StreamWriter writer = new StreamWriter(File.OpenWrite(Path.Combine(output.FullName, fileName + ".csv"))))
                {
                    writer.WriteLine(string.Join(", ", nameof(Person.Name), nameof(Person.Job), nameof(Person.Email), nameof(Person.Campus), nameof(Person.Department)));

                    foreach (var person in people)
                    {
                        writer.WriteLine(person);
                    }
                }

                Console.WriteLine($"Saved {people.Count} results to {fileName}.");
            }
        }
    }
}