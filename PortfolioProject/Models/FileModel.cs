namespace PortfolioProject.Models
{
    public class FileModel(string listing, string path)
    {
        public string Listing { get; set; } = listing;
        public string Path { get; set; } = path;
    }
}
