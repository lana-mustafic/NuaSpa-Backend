using FluentValidation;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Validators;

public sealed class AdminKlijentUpdateDtoValidator : AbstractValidator<AdminKlijentUpdateDto>
{
    private static readonly System.Text.RegularExpressions.Regex EmailRegex =
        new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", System.Text.RegularExpressions.RegexOptions.Compiled);

    private static readonly System.Text.RegularExpressions.Regex PhoneRegex =
        new(@"^\+?[0-9][0-9\s\-]{7,18}$", System.Text.RegularExpressions.RegexOptions.Compiled);

    public AdminKlijentUpdateDtoValidator()
    {
        RuleFor(x => x.Ime)
            .MaximumLength(50).WithMessage("Ime može imati najviše 50 znakova.")
            .When(x => !string.IsNullOrWhiteSpace(x.Ime));

        RuleFor(x => x.Prezime)
            .MaximumLength(50).WithMessage("Prezime može imati najviše 50 znakova.")
            .When(x => !string.IsNullOrWhiteSpace(x.Prezime));

        RuleFor(x => x.Email)
            .Must(e => string.IsNullOrWhiteSpace(e) || EmailRegex.IsMatch(e.Trim()))
            .WithMessage("Unesite ispravnu e-mail adresu u formatu: ime@domena.ba")
            .When(x => x.Email != null);

        RuleFor(x => x.Telefon)
            .Must(t => string.IsNullOrWhiteSpace(t) || PhoneRegex.IsMatch(t.Trim()))
            .WithMessage(
                "Unesite ispravan broj telefona u formatu: +387 61 123 456 ili samo cifre (8–15 znamenki).")
            .When(x => x.Telefon != null);

        RuleFor(x => x.NapomenaZaTerapeuta)
            .MaximumLength(1200).WithMessage("Napomena može imati najviše 1200 znakova.")
            .When(x => x.NapomenaZaTerapeuta != null);

        RuleFor(x => x.NovaLozinka)
            .MinimumLength(6)
            .WithMessage("Nova lozinka mora imati najmanje 6 znakova.")
            .When(x => !string.IsNullOrWhiteSpace(x.NovaLozinka));

        RuleFor(x => x)
            .Must(x =>
                string.IsNullOrWhiteSpace(x.NovaLozinka) &&
                string.IsNullOrWhiteSpace(x.PotvrdaNoveLozinke))
            .WithMessage("Potvrda lozinke je obavezna kada mijenjate lozinku.")
            .When(x =>
                !string.IsNullOrWhiteSpace(x.NovaLozinka) &&
                string.IsNullOrWhiteSpace(x.PotvrdaNoveLozinke));

        RuleFor(x => x)
            .Must(x =>
                string.IsNullOrWhiteSpace(x.NovaLozinka) ||
                x.NovaLozinka == x.PotvrdaNoveLozinke)
            .WithMessage("Nova lozinka i potvrda se ne podudaraju.")
            .When(x =>
                !string.IsNullOrWhiteSpace(x.NovaLozinka) ||
                !string.IsNullOrWhiteSpace(x.PotvrdaNoveLozinke));
    }
}
