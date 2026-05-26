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
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("Email nije ispravan.");

        RuleFor(x => x.KategorijaUslugaId)
            .GreaterThan(0).When(x => x.KategorijaUslugaId.HasValue)
            .WithMessage("KategorijaUslugaId mora biti pozitivan broj.");
    }
}
