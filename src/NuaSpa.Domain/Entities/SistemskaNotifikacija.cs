using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NuaSpa.Domain.Common;
using NuaSpa.Domain.Enums;

namespace NuaSpa.Domain.Entities;

/// <summary>In-app sistemska obavijest za korisnika (pročitano/nepročitano).</summary>
public class SistemskaNotifikacija : BaseEntity
{
    public int KorisnikId { get; set; }
    public Korisnik? Korisnik { get; set; }

    public SistemskaNotifikacijaTip Tip { get; set; }

    [Required]
    [MaxLength(200)]
    public string Naslov { get; set; } = null!;

    [Required]
    [MaxLength(2000)]
    public string Tekst { get; set; } = null!;

    public bool Procitana { get; set; }

    public int? RezervacijaId { get; set; }
    public Rezervacija? Rezervacija { get; set; }
}
