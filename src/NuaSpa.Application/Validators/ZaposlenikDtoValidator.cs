using FluentValidation;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Validators;

public sealed class ZaposlenikDtoValidator : AbstractValidator<ZaposlenikDTO>
{
    public ZaposlenikDtoValidator()
    {
        RuleFor(x => x.Ime)
            .NotEmpty().WithMessage("Ime je obavezno.")
            .MaximumLength(50);

        RuleFor(x => x.Prezime)
            .NotEmpty().WithMessage("Prezime je obavezno.")
            .MaximumLength(50);

        RuleFor(x => x.Specijalizacija)
            .NotEmpty().WithMessage("Specijalizacija je obavezna.")
            .MaximumLength(500);

        RuleFor(x => x.Email)
            .Must(e => string.IsNullOrWhiteSpace(e) ||
                       System.Text.RegularExpressions.Regex.IsMatch(
                           e.Trim(), @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
            .WithMessage("Unesite ispravnu e-mail adresu u formatu: ime@domena.ba")
            .When(x => x.Email != null);

        RuleFor(x => x.Telefon)
            .Must(t => string.IsNullOrWhiteSpace(t) ||
                       System.Text.RegularExpressions.Regex.IsMatch(
                           t.Trim(), @"^\+?[0-9][0-9\s\-]{7,18}$"))
            .WithMessage(
                "Unesite ispravan broj telefona u formatu: +387 61 123 456 ili samo cifre (8–15 znamenki).")
            .When(x => x.Telefon != null);

        RuleFor(x => x.KategorijaUslugaId)
            .GreaterThan(0).When(x => x.KategorijaUslugaId.HasValue)
            .WithMessage("KategorijaUslugaId mora biti pozitivan broj.");
    }
}
