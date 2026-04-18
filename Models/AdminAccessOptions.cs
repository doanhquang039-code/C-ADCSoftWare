namespace WEBDULICH.Models
{
    public class AdminAccessOptions
    {
        public const string SectionName = "AdminAccess";

        public List<string> Emails { get; set; } = new();
    }
}
