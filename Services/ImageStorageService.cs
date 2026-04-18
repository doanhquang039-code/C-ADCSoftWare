namespace WEBDULICH.Services
{
    public class ImageStorageService : IImageStorageService
    {
        private readonly IWebHostEnvironment environment;

        public ImageStorageService(IWebHostEnvironment environment)
        {
            this.environment = environment;
        }

        public async Task<string?> SaveAsync(IFormFile? imageFile, string folderName)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return null;
            }

            var extension = Path.GetExtension(imageFile.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var targetFolder = Path.Combine(environment.WebRootPath, folderName);

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            var filePath = Path.Combine(targetFolder, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await imageFile.CopyToAsync(stream);
            return fileName;
        }

        public void Delete(string folderName, string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            var filePath = Path.Combine(environment.WebRootPath, folderName, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
