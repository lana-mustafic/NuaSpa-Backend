using System.ComponentModel.DataAnnotations;

namespace NuaSpa.Application.DTOs;

public class AdminKlijentCreateDto
{
    [Required, MaxLength(50)]
    public string Ime { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Prezime { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(256)]
    public string UserName { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Telefon { get; set; }

    [Range(1, int.MaxValue)]
    public int GradId { get; set; } = 1;

    public bool IsVipKlijent { get; set; }

    [MaxLength(1200)]
    public string? NapomenaZaTerapeuta { get; set; }
}
