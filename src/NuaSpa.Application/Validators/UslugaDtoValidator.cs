using FluentValidation;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Validators;

public sealed class UslugaDtoValidator : AbstractValidator<UslugaDTO>
{
    public UslugaDtoValidator()
    {
        RuleFor(x => x.Naziv)
            .NotEmpty().WithMessage("Naziv usluge je obavezan.")
            .MaximumLength(200)
            .WithMessage("Naziv usluge može imati najviše 200 znakova.");

        RuleFor(x => x.Cijena)
            .GreaterThan(0)
            .WithMessage("Unesite ispravan iznos u KM (npr. 80.00). Iznos mora biti veći od 0.");

        RuleFor(x => x.TrajanjeMinuta)
            .InclusiveBetween(15, 480)
            .WithMessage("Trajanje mora biti između 15 i 480 minuta (unesite cijeli broj, npr. 60).");

        RuleFor(x => x.KategorijaUslugaId)
            .GreaterThan(0).WithMessage("Odaberite kategoriju usluge.");

        RuleFor(x => x.Opis)
            .NotEmpty().WithMessage("Opis usluge je obavezan.")
            .MaximumLength(1000)
            .WithMessage("Opis može imati najviše 1000 znakova.");
    }
}
