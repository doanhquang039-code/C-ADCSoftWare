namespace WEBDULICH.Services
{
    public interface IImageStorageService
    {
        Task<string?> SaveAsync(IFormFile? imageFile, string folderName);
        void Delete(string folderName, string? fileName);
    }
}
