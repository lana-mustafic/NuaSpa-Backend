using FluentValidation;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Validators;

public sealed class AccountProfileUpdateDtoValidator : AbstractValidator<AccountProfileUpdateDto>
{
    private static readonly System.Text.RegularExpressions.Regex EmailRegex =
        new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", System.Text.RegularExpressions.RegexOptions.Compiled);

    private static readonly System.Text.RegularExpressions.Regex PhoneRegex =
        new(@"^\+?[0-9][0-9\s\-]{7,18}$", System.Text.RegularExpressions.RegexOptions.Compiled);

    public AccountProfileUpdateDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ime je obavezno.")
            .MaximumLength(50).WithMessage("Ime može imati najviše 50 znakova.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Prezime je obavezno.")
            .MaximumLength(50).WithMessage("Prezime može imati najviše 50 znakova.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail adresa je obavezna.")
            .MaximumLength(256).WithMessage("E-mail adresa može imati najviše 256 znakova.")
            .Must(e => !string.IsNullOrWhiteSpace(e) && EmailRegex.IsMatch(e.Trim()))
            .WithMessage("Unesite ispravnu e-mail adresu u formatu: ime@domena.ba");

        RuleFor(x => x.Phone)
            .Must(t => string.IsNullOrWhiteSpace(t) || PhoneRegex.IsMatch(t.Trim()))
            .WithMessage(
                "Unesite ispravan broj telefona u formatu: +387 61 123 456 ili samo cifre (8–15 znamenki).");

        RuleFor(x => x.GradId)
            .GreaterThan(0).WithMessage("Grad nije pronađen.")
            .When(x => x.GradId.HasValue);
    }
}
