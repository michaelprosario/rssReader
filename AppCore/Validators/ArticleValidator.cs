using AppCore.Models;
using FluentValidation;

namespace AppCore.Validators
{
    /// <summary>
    /// Validator for Article entity
    /// </summary>
    public class ArticleValidator : AbstractValidator<Article>
    {
        public ArticleValidator()
        {
            RuleFor(a => a.Title)
                .NotEmpty().WithMessage("Article title is required.")
                .MaximumLength(500).WithMessage("Article title cannot exceed 500 characters.");

            RuleFor(a => a.Url)
                .NotEmpty().WithMessage("Article URL is required.")
                .MaximumLength(2048).WithMessage("Article URL cannot exceed 2048 characters.")
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("Article URL must be a valid URL.");

            RuleFor(a => a.Author)
                .MaximumLength(255).WithMessage("Author name cannot exceed 255 characters.");

            RuleFor(a => a.UniqueId)
                .NotEmpty().WithMessage("Article unique ID is required.")
                .MaximumLength(500).WithMessage("Article unique ID cannot exceed 500 characters.");

            RuleFor(a => a.PublishedAt)
                .NotEmpty().WithMessage("Article publication date is required.");
        }
    }
}
