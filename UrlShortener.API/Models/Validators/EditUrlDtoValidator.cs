using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FastEndpoints;
using FluentValidation;
using UrlShortener.API.Models.Dtos;

namespace UrlShortener.API.Models.Validators
{
    public class EditUrlDtoValidator : Validator<EditUrlDto>
    {
        public EditUrlDtoValidator()
        {
            RuleFor(x => x.ShortCode).NotNull().NotEmpty().WithMessage("ShortCode is required!");
            RuleFor(x => x.LongUrl)
                .NotNull()
                .NotEmpty()
                .WithMessage("LongUrl is required!")
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("LongUrl must be url");
            RuleFor(x => x.Expiration)
                .Must(date => date >= DateTime.UtcNow)
                .When(date => date != null)
                .WithMessage("Expiration Date must be greater than or equal to todays date");
            RuleFor(x => x.Status).Must(x => x == true || x == false);
        }
    }
}
