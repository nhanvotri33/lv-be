// ==========================================================================
// MODULE: FileService.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module FileService
// ==========================================================================
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
    // [Hàm thực thi nghiệp vụ]: `Method` - Xử lý logic và luồng dữ liệu
    public interface IFileService
    {
        Task<string> UploadImageAsync(IFormFile file, string subFolder = "general");
        void DeleteImage(string relativePath);
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

        // [Hàm thực thi nghiệp vụ]: `UploadImageAsync` - Xử lý logic và luồng dữ liệu
        public async Task<string> UploadImageAsync(IFormFile file, string subFolder = "general")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ.");

            ValidateImageFile(file);

            return await UploadToLocalAsync(file, subFolder);
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

        private string Slugify(string phrase)
        {
            if (string.IsNullOrEmpty(phrase)) return "";
            
            string str = phrase.ToLowerInvariant();
            
            string[] vietnameseSigns = new string[]
            {
                "aAeEoOuUiIdDyY",
                "áàạảãâấầậẩẫăắằặẳẵ",
                "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                "éèẹẻẽêếềệểễ",
                "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôốồộổỗơớờợởỡ",
                "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỔỠ",
                "úùụủũưứừựửữ",
                "ÚÙỤỦŨƯỨỪỰỬỮ",
                "íìịỉĩ",
                "ÍÌỊỈĨ",
                "đ",
                "Đ",
                "ýỳỵỷỹ",
                "ÝỲỴỶỸ"
            };

            for (int i = 1; i < vietnameseSigns.Length; i++)
            {
                for (int j = 0; j < vietnameseSigns[i].Length; j++)
                {
                    str = str.Replace(vietnameseSigns[i][j], vietnameseSigns[0][i - 1]);
                }
            }

            // Remove invalid characters
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            
            // Convert multiple spaces/hyphens into single hyphen
            str = Regex.Replace(str, @"\s+", " ").Trim();
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            str = Regex.Replace(str, @"\s", "-"); // hyphens
            
            return str;
        }

        // [Hàm thực thi nghiệp vụ]: `DeleteImage` - Xử lý logic và luồng dữ liệu
        public void DeleteImage(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;

            // Loại bỏ các hình ảnh mặc định không được xóa
            var normalizedPath = relativePath.ToLowerInvariant();
            if (normalizedPath.Contains("default") || normalizedPath.Contains("no-image") || normalizedPath.Contains("placeholder"))
            {
                return;
            }

            try
            {
                string cleanRelativePath = relativePath;

                // Nếu là URL tuyệt đối (chứa ://), ta tách lấy phần path sau domain
                if (relativePath.Contains("://"))
                {
                    try
                    {
                        var uri = new Uri(relativePath);
                        cleanRelativePath = uri.AbsolutePath;
                    }
                    catch
                    {
                        int idx = relativePath.IndexOf("uploads/", StringComparison.OrdinalIgnoreCase);
                        if (idx >= 0)
                        {
                            cleanRelativePath = relativePath.Substring(idx);
                        }
                    }
                }

                // Loại bỏ ký tự / ở đầu nếu có để kết hợp đường dẫn chính xác
                cleanRelativePath = cleanRelativePath.TrimStart('/');
                
                string webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string physicalPath = Path.Combine(webRootPath, cleanRelativePath);

                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi xóa file ảnh vật lý: {ex.Message}");
            }
        }

        private async Task<string> UploadToLocalAsync(IFormFile file, string subFolder)
        {
            string webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string uploadFolder = Path.Combine(webRoot, "uploads", subFolder);
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            // ── Deduplication: compute MD5 hash of the incoming file ──────────
            byte[] incomingBytes;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                incomingBytes = ms.ToArray();
            }

            string incomingHash;
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                incomingHash = BitConverter.ToString(md5.ComputeHash(incomingBytes)).Replace("-", "").ToLowerInvariant();
            }

            // Search existing files in the same sub-folder for a byte-identical match
            string fileExtension = Path.GetExtension(file.FileName).ToLower();
            foreach (var existingFile in Directory.EnumerateFiles(uploadFolder))
            {
                // Only compare files with the same extension to short-circuit quickly
                if (!existingFile.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase)) continue;

                byte[] existingBytes = await File.ReadAllBytesAsync(existingFile);
                string existingHash;
                using (var md5 = System.Security.Cryptography.MD5.Create())
                {
                    existingHash = BitConverter.ToString(md5.ComputeHash(existingBytes)).Replace("-", "").ToLowerInvariant();
                }

                if (incomingHash == existingHash)
                {
                    // Identical file already stored — return the existing URL
                    string existingName = Path.GetFileName(existingFile);
                    return $"/uploads/{subFolder}/{existingName}";
                }
            }
            // ─────────────────────────────────────────────────────────────────

            string rawFileName = Path.GetFileNameWithoutExtension(file.FileName);
            string cleanName = Slugify(rawFileName);
            if (string.IsNullOrEmpty(cleanName))
            {
                cleanName = "image";
            }

            string uniqueFileName = $"{cleanName}-{Guid.NewGuid().ToString().Substring(0, 8)}{fileExtension}";
            string filePath = Path.Combine(uploadFolder, uniqueFileName);

            await File.WriteAllBytesAsync(filePath, incomingBytes);

            return $"/uploads/{subFolder}/{uniqueFileName}";
        }
    }
}

