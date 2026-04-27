namespace WEBDULICH.Services
{
    public interface ILocalizationService
    {
        string GetString(string key, string? culture = null);
        string GetString(string key, string culture, params object[] args);
        void SetCulture(string culture);
        string GetCurrentCulture();
        List<string> GetSupportedCultures();
        Dictionary<string, string> GetAllStrings(string culture);
    }
}