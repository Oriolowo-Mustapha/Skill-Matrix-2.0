using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;

namespace Application.Extensions
{
    public static class FileValidationExtensions
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/webp" };
        private const long MaxFileSizeInBytes = 2 * 1024 * 1024; // 2MB

        public static IRuleBuilderOptions<T, IFormFile?> IsValidImage<T>(this IRuleBuilder<T, IFormFile?> ruleBuilder)
        {
            return (IRuleBuilderOptions<T, IFormFile?>)ruleBuilder.Custom<T, IFormFile?>((file, context) =>
            {
                if (file == null)
                {
                    return;
                }

                if (file.Length == 0)
                {
                    context.AddFailure("File cannot be empty.");
                    return;
                }

                if (file.Length > MaxFileSizeInBytes)
                {
                    context.AddFailure("File size cannot exceed 2 MB.");
                    return;
                }

                var extension = Path.GetExtension(file.FileName)?.ToLower();
                if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
                {
                    context.AddFailure($"Invalid file extension. Allowed extensions are: {string.Join(", ", AllowedExtensions)}");
                    return;
                }

                var contentType = file.ContentType?.ToLower();
                if (string.IsNullOrEmpty(contentType) || !AllowedMimeTypes.Contains(contentType))
                {
                    context.AddFailure($"Invalid MIME type. Allowed MIME types are: {string.Join(", ", AllowedMimeTypes)}");
                    return;
                }
            });
        }
    }
}
