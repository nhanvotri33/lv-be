using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ECommerce1.Services
{
    public interface IFileService
    {
        Task<string> UploadImageAsync(IFormFile file);
    }

    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public FileService(IWebHostEnvironment env, IConfiguration config)
        {
            _env = env;
            _config = config;
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ.");

            ValidateImageFile(file);

            return await UploadToLocalAsync(file);
        }

        private void ValidateImageFile(IFormFile file)
        {
            // 1. Check extension
            var extension = Path.GetExtension(file.FileName).ToLower();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".svg" };
            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("Định dạng file không được hỗ trợ. Chỉ hỗ trợ .jpg, .jpeg, .png, .webp, .svg");

            // 2. Check Content-Type (MIME)
            var mimeType = file.ContentType.ToLower();
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/svg+xml", "image/pjpeg", "image/x-png" };
            if (!allowedMimeTypes.Contains(mimeType))
                throw new ArgumentException("MIME type của file không hợp lệ.");

            // 3. Prevent extension mismatch spoofing (Check file signature)
            byte[] header = new byte[12];
            using (var stream = file.OpenReadStream())
            {
                stream.Read(header, 0, header.Length);
            }

            if (extension == ".jpg" || extension == ".jpeg")
            {
                if (header[0] != 0xFF || header[1] != 0xD8)
                    throw new ArgumentException("Cấu trúc file JPEG không hợp lệ.");
            }
            else if (extension == ".png")
            {
                if (header[0] != 0x89 || header[1] != 0x50 || header[2] != 0x4E || header[3] != 0x47)
                    throw new ArgumentException("Cấu trúc file PNG không hợp lệ.");
            }
            else if (extension == ".webp")
            {
                string riff = System.Text.Encoding.ASCII.GetString(header, 0, 4);
                string webp = System.Text.Encoding.ASCII.GetString(header, 8, 4);
                if (riff != "RIFF" || webp != "WEBP")
                    throw new ArgumentException("Cấu trúc file WEBP không hợp lệ.");
            }
            else if (extension == ".svg")
            {
                string svgStart = System.Text.Encoding.UTF8.GetString(header, 0, header.Length).TrimStart().ToLowerInvariant();
                if (!svgStart.StartsWith("<") && !svgStart.StartsWith("?xml") && !svgStart.StartsWith("<!doc"))
                    throw new ArgumentException("Cấu trúc file SVG không đúng định dạng XML.");

                // SVG XSS Check
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    var content = reader.ReadToEnd();
                    var contentLower = content.ToLowerInvariant();
                    
                    if (contentLower.Contains("<script") || contentLower.Contains("</script>"))
                        throw new ArgumentException("File SVG chứa mã script không an toàn.");

                    if (Regex.IsMatch(contentLower, @"\bon[a-z]+\s*="))
                        throw new ArgumentException("File SVG chứa thuộc tính sự kiện (event handler) không an toàn.");

                    if (contentLower.Contains("javascript:"))
                        throw new ArgumentException("File SVG chứa liên kết javascript: không an toàn.");
                }
            }
        }

        private async Task<string> UploadToLocalAsync(IFormFile file)
        {
            string uploadFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            string fileExtension = Path.GetExtension(file.FileName);
            string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
            string filePath = Path.Combine(uploadFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/{uniqueFileName}";
        }
    }
}

