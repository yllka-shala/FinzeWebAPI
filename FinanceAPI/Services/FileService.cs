using FinanceAPI.Helpers;
using Microsoft.AspNetCore.Hosting;

namespace FinanceAPI.Services
{
    public class FileService : IFileService
    {
        private readonly ILogger<FileService> _logger;

        public FileService(ILogger<FileService> logger) 
        {
            _logger = logger;
        }

        public async Task<string> SaveFile(IFormFile file)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string[] allowedExtensions = [".png", ".jpg"];
            var extension = Path.GetExtension(file.FileName);

            if (!allowedExtensions.Contains(extension))
            {
                _logger.LogWarning("File upload failed due to unsupported extension: {@Extension}. Allowed: {@AllowedExtensions}", extension, string.Join(",", allowedExtensions));
                throw new FileLoadException($"Only {string.Join(",", allowedExtensions)}");
            }

            var newFileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(path, newFileName);

            using var fileStream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(fileStream);

            return newFileName;
        }

        public void DeleteFile(string fileName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            var fullPath = Path.Combine(path, fileName);

            if (!Path.Exists(fullPath))
            {
                _logger.LogWarning("Requested file '{@FileName}' not found at path: {@FullPath}", fileName, fullPath);
                throw new FileNotFoundException($"File {fileName} does not exist!");
            }

            File.Delete(fullPath);
        }
}
}
