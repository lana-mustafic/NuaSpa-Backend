using FluentValidation;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Validators;

public sealed class KategorijaUslugaDtoValidator : AbstractValidator<KategorijaUslugaDTO>
{
    public KategorijaUslugaDtoValidator()
    {
        RuleFor(x => x.Naziv)
            .NotEmpty().WithMessage("Naziv kategorije je obavezan.")
            .MaximumLength(100);
    }
}
