using FluentValidation;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Validators;

public sealed class UslugaDtoValidator : AbstractValidator<UslugaDTO>
{
    public UslugaDtoValidator()
    {
        RuleFor(x => x.Naziv)
            .NotEmpty().WithMessage("Naziv usluge je obavezan.")
            .MaximumLength(200);

        RuleFor(x => x.Cijena)
            .GreaterThan(0).WithMessage("Cijena mora biti veća od nule.");

        RuleFor(x => x.TrajanjeMinuta)
            .InclusiveBetween(15, 480).WithMessage("Trajanje mora biti između 15 i 480 minuta.");

        RuleFor(x => x.KategorijaUslugaId)
            .GreaterThan(0).WithMessage("Kategorija usluge je obavezna.");

        RuleFor(x => x.Opis)
            .NotEmpty().WithMessage("Opis usluge je obavezan.")
            .MaximumLength(1000);
    }
}
