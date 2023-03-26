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

                    string query = settings.SluSearchUrl.Replace("DEPARTMENTNAME", Uri.EscapeDataString(department)).Replace("LOCATION", campus.Name);
                    Console.WriteLine($"Getting results from URL '{query}'");

                    string webpage = LoadWebpage(query);

                    Console.WriteLine("Data received.");

                    string filename = CreateHtmlCacheFileName(campus.Name, department);
                    SaveToCacheAsHtml(filename, webpage);

                    Console.WriteLine($"Data saved to '{Path.Combine(settings.CachedSearchResultsDirectory, filename)}'.");
                    Console.WriteLine("Waiting briefly between queries...");

                    Thread.Sleep(3000);
                }
            }
            
            Console.WriteLine("Done.");

            settings.Save();
        }

        private string LoadWebpage(string url)
        {
            using (var client = new HttpClient())
            {
                var message = new HttpRequestMessage(HttpMethod.Get, url);
                var response = client.Send(message);
                response.EnsureSuccessStatusCode();

                using (var reader = new StreamReader(response.Content.ReadAsStream()))
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

            using (var writer = File.CreateText(Path.Combine(settings.CachedSearchResultsDirectory, fileName)))
            {
                writer.Write(content);
            }
        }

        private string CreateHtmlCacheFileName(string campus, string department)
        {
            return $"{campus}_{department}.html";
        }

        [Command("parse-from-cache", Description = CommandDescriptions.PARSE_FROM_CACHE, ExtendedHelpText = CommandDescriptions.PARSE_FROM_CACHE_EXTENDED)]
        public void ParseFromCache()
        {
            var cache = new DirectoryInfo(settings.CachedSearchResultsDirectory);

            if (cache.Exists)
            {
                ICollection<Person> people = new List<Person>();

                foreach (var campus in settings.TargetCampuses)
                {
                    foreach (var department in campus.Departments)
                    {
                        var cachedHtml = new FileInfo(Path.Combine(cache.FullName, CreateHtmlCacheFileName(campus.Name, department)));

                        if (cachedHtml.Exists)
                        {
                            var workingHtml = new HtmlDocument();
                            workingHtml.Load(cachedHtml.OpenRead());

                            if (workingHtml.ParsedText.Contains("We couldn&#39;t find any contact information with your choice of search parameters."))
                            {
                                Console.WriteLine($"No search results found at {campus}: {department}");
                            }
                            else
                            {
                                var searchResults = workingHtml.DocumentNode.SelectNodes(settings.SearchResultXPath);

                                if (searchResults is null)
                                {
                                    Console.WriteLine($"At {campus}: {department}, XPath selector '{settings.SearchResultXPath}' failed to select any results.");
                                }
                                else
                                {
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
                        else
                        {
                            Console.WriteLine($"Expected file {cachedHtml.FullName} does not exist. Please run 'update-cache'.");
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

            if (string.IsNullOrEmpty(name))
            {
                Console.WriteLine($"Failed to parse employee name with XPath '{settings.NameXPath}'.");
            }

            if (string.IsNullOrEmpty(email))
            {
                Console.WriteLine($"Failed to parse employee email for {name} with XPath '{settings.EmailXPath}'.");
            }

            if (string.IsNullOrEmpty(job))
            {
                Console.WriteLine($"Failed to parse employee job for {name} with XPath '{settings.JobXPath}'.");
            }

            return new Person(name, job, email, campus, department);
        }

        private void SavePeopleToOutputDirectoryAsCsv(string fileName, ICollection<Person> people)
        {
            var outputFolder = Directory.CreateDirectory(settings.OutputDirectory);

            var fullPathToFile = Path.Combine(outputFolder.FullName, fileName + ".csv");

            using (var fileStream = File.OpenWrite(fullPathToFile))
            {
                WritePeopleToStreamAsCsv(fileStream, people);
            }
            
            Console.WriteLine($"{people.Count} results saved to {fullPathToFile}.");
        }

        private void WritePeopleToStreamAsCsv(Stream stream, ICollection<Person> people)
        {
            if (people.Any())
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.WriteLine(string.Join(", ", nameof(Person.Name), nameof(Person.Job), nameof(Person.Email), nameof(Person.Campus), nameof(Person.Department)));

                    foreach (var person in people)
                    {
                        writer.WriteLine(person);
                    }
                }
            }
        }
    }
}