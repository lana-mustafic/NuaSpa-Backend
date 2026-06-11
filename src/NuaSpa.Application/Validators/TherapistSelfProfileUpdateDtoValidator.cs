using FluentValidation;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Validators;

public sealed class TherapistSelfProfileUpdateDtoValidator
    : AbstractValidator<TherapistSelfProfileUpdateDto>
{
  private static readonly System.Text.RegularExpressions.Regex PhoneRegex =
      new(@"^\+?[0-9][0-9\s\-]{7,18}$", System.Text.RegularExpressions.RegexOptions.Compiled);

    public TherapistSelfProfileUpdateDtoValidator()
    {
        RuleFor(x => x.Telefon)
            .Must(t => string.IsNullOrWhiteSpace(t) || PhoneRegex.IsMatch(t.Trim()))
            .WithMessage(
                "Enter a valid phone number, e.g. +387 61 123 456 or digits only (8–15 digits).")
            .When(x => x.Telefon != null);

        RuleFor(x => x.Jezici)
            .MaximumLength(200)
            .When(x => x.Jezici != null);

        RuleFor(x => x.Bio)
            .MaximumLength(2000)
            .When(x => x.Bio != null);
    }
}
