using FluentValidation;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Validators;

public sealed class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordDtoValidator()
    {
        RuleFor(x => x.StaraLozinka)
            .NotEmpty().WithMessage("Unesite trenutnu lozinku.");

        RuleFor(x => x.NovaLozinka)
            .NotEmpty().WithMessage("Unesite novu lozinku.")
            .MinimumLength(6).WithMessage("Nova lozinka mora imati najmanje 6 znakova.");

        RuleFor(x => x.PotvrdaNoveLozinke)
            .NotEmpty().WithMessage("Potvrdite novu lozinku.");

        RuleFor(x => x)
            .Must(x => x.NovaLozinka == x.PotvrdaNoveLozinke)
            .WithMessage("Nova lozinka i potvrda se ne podudaraju.");

        RuleFor(x => x)
            .Must(x => x.StaraLozinka != x.NovaLozinka)
            .WithMessage("Nova lozinka mora biti različita od trenutne.");
    }
}
