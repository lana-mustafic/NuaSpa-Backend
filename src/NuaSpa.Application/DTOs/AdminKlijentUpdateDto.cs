using System.ComponentModel.DataAnnotations;

namespace NuaSpa.Application.DTOs;

public class AdminKlijentUpdateDto
{
    [MaxLength(50)]
    public string? Ime { get; set; }

    [MaxLength(50)]
    public string? Prezime { get; set; }

    [EmailAddress]
    [MaxLength(256)]
    public string? Email { get; set; }

    [MaxLength(50)]
    public string? Telefon { get; set; }

    /// <summary>Account active flag (false = deactivated, history retained).</summary>
    public bool? Status { get; set; }

    public bool? IsVipKlijent { get; set; }

    public int? GradId { get; set; }

    public int? ZaposlenikId { get; set; }

    [MaxLength(1200)]
    public string? NapomenaZaTerapeuta { get; set; }

    /// <summary>Nova lozinka (samo ako admin mijenja lozinku; stara lozinka nije potrebna).</summary>
    public string? NovaLozinka { get; set; }

    /// <summary>Potvrda nove lozinke (obavezna ako je NovaLozinka postavljena).</summary>
    public string? PotvrdaNoveLozinke { get; set; }
}
