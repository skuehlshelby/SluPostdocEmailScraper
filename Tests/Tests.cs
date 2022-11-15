using SluEmailScraper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void TestCaching()
        {
            Program program = new Program();
            program.ScrapeFromSluWebpageAndSaveToCache();
        }

        [TestMethod]
        public void TestParsingFromCache()
        {
            Program program = new Program();
            program.ParseFromCache();
        }

        [TestMethod]
        public void TestSettingsSave()
        {   
            Settings settings = new Settings();
            settings.Save();
        }
    }
}