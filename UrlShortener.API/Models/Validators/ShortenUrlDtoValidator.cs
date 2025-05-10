using FastEndpoints;
using FluentValidation;
using UrlShortener.API.Models.Dtos;

namespace UrlShortener.API.Models.Validators
{
    public class ShortenUrlDtoValidator : Validator<ShortenUrlDto>
    {
        public ShortenUrlDtoValidator()
        {
            RuleFor(x => x.LongUrl)
                .NotEmpty()
                .WithMessage("LongUrl is required!")
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("LongUrl must be url");
            RuleFor(x => x.Expiration)
                .Must(date => date >= DateTime.UtcNow)
                .When(date => date != null)
                .WithMessage("Expiration Date must be greater than or equal to todays date");
        }
    }
}
