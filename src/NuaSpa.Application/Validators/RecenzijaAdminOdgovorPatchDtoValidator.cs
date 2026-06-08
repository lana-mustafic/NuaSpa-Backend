using FluentValidation;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Validators;

public sealed class RecenzijaAdminOdgovorPatchDtoValidator
    : AbstractValidator<RecenzijaAdminOdgovorPatchDto>
{
    public RecenzijaAdminOdgovorPatchDtoValidator()
    {
        RuleFor(x => x.Tekst)
            .MaximumLength(2000)
            .WithMessage("Salon response can be at most 2000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Tekst));
    }
}
