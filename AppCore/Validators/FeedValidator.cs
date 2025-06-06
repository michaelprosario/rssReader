using AppCore.Models.Feeds;
using FluentValidation;

namespace AppCore.Validators
{
    /// <summary>
    /// Validator for Feed entity
    /// </summary>
    public class FeedValidator : AbstractValidator<Feed>
    {
        public FeedValidator()
        {
            RuleFor(f => f.Title)
                .NotEmpty().WithMessage("Feed title is required.")
                .MaximumLength(255).WithMessage("Feed title cannot exceed 255 characters.");

            RuleFor(f => f.FeedUrl)
                .NotEmpty().WithMessage("Feed URL is required.")
                .MaximumLength(2048).WithMessage("Feed URL cannot exceed 2048 characters.")
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("Feed URL must be a valid URL.");

            RuleFor(f => f.WebsiteUrl)
                .MaximumLength(2048).WithMessage("Website URL cannot exceed 2048 characters.")
                .Must(uri => string.IsNullOrEmpty(uri) || Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("Website URL must be a valid URL if provided.");

            RuleFor(f => f.Description)
                .MaximumLength(1000).WithMessage("Feed description cannot exceed 1000 characters.");

            RuleFor(f => f.RefreshIntervalMinutes)
                .GreaterThan(0).When(f => f.RefreshIntervalMinutes.HasValue)
                .WithMessage("Refresh interval must be greater than 0 minutes.");
        }
    }
}
