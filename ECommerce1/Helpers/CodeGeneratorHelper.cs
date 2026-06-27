using System;
using System.Text;
using System.Linq;

namespace ECommerce1.Helpers
{
    public static class CodeGeneratorHelper
    {
        public static string GenerateBrandOrCategoryCode(string name, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(name)) return "CODE";
            string cleanName = RemoveDiacriticsAndSpecialChars(name);
            var words = cleanName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string code = "";
            if (words.Length == 1)
            {
                code = words[0].Length > maxLength ? words[0].Substring(0, maxLength) : words[0];
            }
            else
            {
                foreach (var word in words)
                {
                    if (!string.IsNullOrEmpty(word)) code += word[0];
                }
            }
            code = code.ToUpper();
            return code.Length > maxLength ? code.Substring(0, maxLength) : code;
        }

        public static string GenerateProductCode(string name, int maxLength = 20)
        {
            if (string.IsNullOrWhiteSpace(name)) return "PROD";
            string cleanName = RemoveDiacriticsAndSpecialCharsAllowNumbers(name);
            var words = cleanName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string code = "";
            foreach (var word in words)
            {
                if (word.Any(char.IsDigit))
                {
                    code += word;
                }
                else
                {
                    code += word[0];
                }
            }
            code = code.ToUpper();
            return code.Length > maxLength ? code.Substring(0, maxLength) : code;
        }

        private static string RemoveDiacriticsAndSpecialChars(string text)
        {
            // Thay thế chữ Đ, đ trước
            text = text.Replace('đ', 'd').Replace('Đ', 'D');
            string normalizedString = text.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    if (char.IsLetter(c) || char.IsWhiteSpace(c))
                        stringBuilder.Append(c);
                }
            }
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        private static string RemoveDiacriticsAndSpecialCharsAllowNumbers(string text)
        {
            text = text.Replace('đ', 'd').Replace('Đ', 'D');
            string normalizedString = text.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
                        stringBuilder.Append(c);
                }
            }
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
