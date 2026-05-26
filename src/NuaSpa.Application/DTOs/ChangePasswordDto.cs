using System.ComponentModel.DataAnnotations;

namespace NuaSpa.Application.DTOs;

/// <summary>Promjena vlastite lozinke (korisnik mora potvrditi staru).</summary>
public class ChangePasswordDto
{
    [Required]
    public string StaraLozinka { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string NovaLozinka { get; set; } = string.Empty;

    [Required]
    public string PotvrdaNoveLozinke { get; set; } = string.Empty;
}
