using FileStorage.Application.DTOs;
using FluentValidation;

namespace FileStorage.Application.Validators;


public class FileUploadValidator : AbstractValidator<FileUploadDto>
{
    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        // Images
        "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp", "image/bmp",
        // Documents
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.ms-powerpoint",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        // Text
        "text/plain", "text/csv",
        // Archives
        "application/zip", "application/x-zip-compressed",
        "application/x-rar-compressed",
        // Other
        "application/json", "application/xml", "text/xml"
    };

    private const long MaxFileSize = 100 * 1024 * 1024; // 100 MB

    public FileUploadValidator()
    {
        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File is required");

        RuleFor(x => x.File)
            .Must(file => file != null && file.Length > 0)
            .WithMessage("File cannot be empty")
            .When(x => x.File != null);

        RuleFor(x => x.File!)
            .Must(file => file.Length <= MaxFileSize)
            .WithMessage($"File size cannot exceed {MaxFileSize / (1024 * 1024)} MB")
            .When(x => x.File != null);

        RuleFor(x => x.File!)
            .Must(file => AllowedMimeTypes.Contains(file.ContentType))
            .WithMessage("File type is not allowed. Allowed types: images, PDFs, Office documents, text files, and archives.")
            .When(x => x.File != null);

        RuleFor(x => x.File!)
            .Must(file => !string.IsNullOrWhiteSpace(file.FileName))
            .WithMessage("File name is required")
            .When(x => x.File != null);

        RuleFor(x => x.Tags)
            .MaximumLength(2000)
            .WithMessage("Tags cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Tags));
    }
}

