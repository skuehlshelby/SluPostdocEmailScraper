namespace SluEmailScraper
{
    internal static class CommandDescriptions
    {
        public const string UPDATE_CACHE = "Fetch search results from slu.se";
        public const string UPDATE_CACHE_EXTENDED =
            "Fetches the latest staff entries from slu.se as raw HTML pages and saves them to disk. " +
            "This is done to reduce the likleyhood of SLU taking offense to what we are doing by keeping " +
            "additional traffic to a minimum. After running this command, use 'parse-from-cache' to obtain " +
            "the actual results as .csv files.";

        public const string PARSE_FROM_CACHE = "Turn the previously saved search results into .csv files.";
        public const string PARSE_FROM_CACHE_EXTENDED = 
            "Parses the HTML files saved to the cache, and returns the results as .csv files. If there are no " +
            "HTML files in the cache, the cache is out of date, or the cache does not exist, run 'update-cache'.";
    }
}
