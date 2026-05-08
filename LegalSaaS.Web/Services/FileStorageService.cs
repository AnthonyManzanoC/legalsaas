using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LegalSaaS.Web.Services
{
    public interface IFileStorageService
    {
        Task<string> SaveImageAsync(IBrowserFile file, string folder);
    }

    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;

        public FileStorageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveImageAsync(IBrowserFile file, string folder)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.Name).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                throw new InvalidOperationException("Formato de imagen no permitido.");

            var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsPath = Path.Combine(webRootPath, "uploads", folder);
            Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(uploadsPath, fileName);

            await using var stream = file.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024);
            await using var fs = new FileStream(fullPath, FileMode.Create);
            await stream.CopyToAsync(fs);

            return $"/uploads/{folder}/{fileName}";
        }
    }
}