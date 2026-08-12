using ECommerce.Models;
using ECommerce1.Controllers;
using ECommerce1.DTOs.Blog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ECommerce1.Controllers
{
    public static class BlogControllerHelpers
    {
        public static string GenerateSlug(string phrase)
        {
            if (string.IsNullOrWhiteSpace(phrase)) return "";
            string str = phrase.ToLower().Trim();
            // Thay thế ký tự tiếng Việt
            string[] vietnameseSigns = new string[]
            {
                "aàảãáạăằẳẵắặâầẩẫấậ", "a",
                "dđ", "d",
                "eèẻẽéẹêềểễếệ", "e",
                "iìỉĩíị", "i",
                "oòỏõóọôồổỗốộơờởỡớợ", "o",
                "uùủũúụưừửữứự", "u",
                "yỳỷỹýỵ", "y"
            };

            for (int i = 1; i < vietnameseSigns.Length; i += 2)
            {
                for (int j = 0; j < vietnameseSigns[i - 1].Length; j++)
                {
                    str = str.Replace(vietnameseSigns[i - 1][j], vietnameseSigns[i][0]);
                }
            }

            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", " ").Trim();
            str = Regex.Replace(str, @"\s", "-");
            return str;
        }

        public static BlogResponse MapToResponse(Blog b)
        {
            if (b == null) return null;
            return new BlogResponse
            {
                Id = b.Id,
                Title = b.Title,
                Slug = b.Slug,
                Summary = b.Summary,
                Content = b.Content,
                ThumbnailUrl = b.ThumbnailUrl,
                Author = b.Author,
                Category = b.Category,
                Tags = b.Tags,
                ViewCount = b.ViewCount,
                IsPublished = b.IsPublished,
                IsFeatured = b.IsFeatured,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt,
                UserId = b.UserId,
                AuthorName = b.User != null ? b.User.Username : b.Author
            };           
        }
    }
}