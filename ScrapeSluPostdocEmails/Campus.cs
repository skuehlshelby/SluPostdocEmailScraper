namespace SluEmailScraper
{
    public class Campus
    {
        public string Name { get; set; }
        public string[] Departments { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
