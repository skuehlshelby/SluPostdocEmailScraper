﻿using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace SluEmailScraper
{
    public sealed class Settings
    {
        private static readonly string AppDirectory = GetSettingsDirectory();
        private static readonly string SettingsFile = Path.Combine(GetSettingsDirectory(), "settings.xml");
        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(Settings));

        private static string GetSettingsDirectory()
        {
            string appName = Assembly.GetExecutingAssembly().GetName().Name;
            string myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            return Path.Combine(myDocuments, appName);
        }

        public Settings()
        {
            CachedSearchResultsDirectory = Path.Combine(AppDirectory, "Cache");
            OutputDirectory = Path.Combine(AppDirectory, "Output");
            SluSearchUrl = @"https://www.slu.se/en/settings/employee-search/?firstName=&lastName=&telephoneNumber=&email=&ou=DEPARTMENTNAME&location=";
            SearchResultXPath = @"//div[@class='row result']";
            NameXPath = @".//span[@class='result-name']";
            EmailXPath = @".//a[starts-with(@href, 'mailto')]";
            JobKeyPhrase = "at the";
            JobXPath = $".//span[contains(text(), '{JobKeyPhrase}')]";
            TargetDepartments = new string[] 
            {
                "Enheten för hippologutbildning",
                "Enheten för skoglig fältforskning",
                "Institutionen för akvatiska resurser (SLU Aqua)",
                "Institutionen för anatomi, fysiologi och biokemi",
                "Institutionen för biomedicin och veterinär folkhälsovetenskap",
                "Institutionen för biosystem och teknologi",
                "Institutionen för ekologi",
                "Institutionen för ekonomi",
                "Institutionen för energi och teknik",
                "Institutionen för husdjurens miljö och hälsa",
                "Institutionen för husdjurens utfodring och vård",
                "Institutionen för husdjursgenetik",
                "Institutionen för kliniska vetenskaper",
                "Institutionen för landskapsarkitektur, planering och förvaltning",
                "Institutionen för mark och miljö",
                "Institutionen för molekylära vetenskaper",
                "Institutionen för människa och samhälle",
                "Norrländsk jordbruksvetenskap",
                "Institutionen för skogens biomaterial och teknologi (SBT)",
                "Institutionen för skogens ekologi och skötsel",
                "Institutionen för skogens produkter",
                "Institutionen för skoglig genetik och växtfysiologi",
                "Skoglig mykologi och växtpatologi",
                "Institutionen för skoglig resurshushållning",
                "Institutionen för skogsekonomi",
                "Institutionen för stad och land",
                "Institutionen för sydsvensk skogsvetenskap",
                "Institutionen för vatten och miljö",
                "Institutionen för vilt, fisk och miljö",
                "Institutionen för växtbiologi",
                "Institutionen för växtförädling",
                "Institutionen för växtproduktionsekologi",
                "Institutionen för växtskyddsbiologi",
                "Skogsmästarskolan"
             };
            TargetJobs = new string[] { "Postdoc", "Postdoctor" };
        }

        public string CachedSearchResultsDirectory { get; set; }

        public string OutputDirectory { get; set; }

        public string SluSearchUrl { get; set; }

        public string[] TargetDepartments { get; set; }

        public string SearchResultXPath { get; set; }

        public string NameXPath { get; set; }

        public string EmailXPath { get; set; }

        public string JobXPath { get; set; }

        public string JobKeyPhrase { get; set; }

        public string[] TargetJobs { get; set; }

        public static Settings Load()
        {
            if (File.Exists(SettingsFile))
            {
                using (XmlReader reader = XmlReader.Create(SettingsFile))
                {
                    return (Settings)Serializer.Deserialize(reader);
                }
            }
            else
            {
                return new Settings();
            }           
        }

        public void Save()
        {
            XmlWriterSettings writerSettings = new XmlWriterSettings()
            {
                Indent = true,
                OmitXmlDeclaration = true,
                NewLineOnAttributes = true,
            };

            using (XmlWriter writer = XmlWriter.Create(SettingsFile, writerSettings))
            {
                Serializer.Serialize(writer, this);
            }
        }
    }
}
