using FluentValidation;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Validators;

public sealed class AdminKlijentCreateDtoValidator : AbstractValidator<AdminKlijentCreateDto>
{
    private static readonly System.Text.RegularExpressions.Regex EmailRegex =
        new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", System.Text.RegularExpressions.RegexOptions.Compiled);

    private static readonly System.Text.RegularExpressions.Regex PhoneRegex =
        new(@"^\+?[0-9][0-9\s\-]{7,18}$", System.Text.RegularExpressions.RegexOptions.Compiled);

    public AdminKlijentCreateDtoValidator()
    {
        RuleFor(x => x.Ime)
            .NotEmpty().WithMessage("Ime je obavezno.")
            .MaximumLength(50).WithMessage("Ime može imati najviše 50 znakova.");

        RuleFor(x => x.Prezime)
            .NotEmpty().WithMessage("Prezime je obavezno.")
            .MaximumLength(50).WithMessage("Prezime može imati najviše 50 znakova.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail adresa je obavezna.")
            .MaximumLength(256).WithMessage("E-mail adresa može imati najviše 256 znakova.")
            .Must(e => EmailRegex.IsMatch(e.Trim()))
            .WithMessage("Unesite ispravnu e-mail adresu u formatu: ime@domena.ba");

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Korisničko ime je obavezno.")
            .MaximumLength(256).WithMessage("Korisničko ime može imati najviše 256 znakova.")
            .Matches(@"^[\w.\-]+$")
            .WithMessage("Korisničko ime smije sadržavati slova, brojeve, tačku, crticu i donju crtu.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Lozinka je obavezna.")
            .MinimumLength(6)
            .WithMessage("Lozinka mora imati najmanje 6 znakova.");

        RuleFor(x => x.Telefon)
            .Must(t => string.IsNullOrWhiteSpace(t) || PhoneRegex.IsMatch(t.Trim()))
            .WithMessage(
                "Unesite ispravan broj telefona u formatu: +387 61 123 456 ili samo cifre (8–15 znamenki).");

        RuleFor(x => x.GradId)
            .GreaterThan(0).WithMessage("GradId mora biti pozitivan broj.");

        RuleFor(x => x.ZaposlenikId)
            .GreaterThan(0).When(x => x.ZaposlenikId.HasValue)
            .WithMessage("ZaposlenikId mora biti pozitivan broj.");

        RuleFor(x => x.NapomenaZaTerapeuta)
            .MaximumLength(1200).WithMessage("Napomena može imati najviše 1200 znakova.")
            .When(x => x.NapomenaZaTerapeuta != null);
    }
}
