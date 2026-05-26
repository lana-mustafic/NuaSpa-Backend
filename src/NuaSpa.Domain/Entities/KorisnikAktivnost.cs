using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NuaSpa.Domain.Common;
using NuaSpa.Domain.Enums;

namespace NuaSpa.Domain.Entities;

/// <summary>Persistirana aktivnost korisnika (pretraga, pregled usluge) za recommender.</summary>
public class KorisnikAktivnost : BaseEntity
{
    [Required]
    [ForeignKey(nameof(Korisnik))]
    public int KorisnikId { get; set; }
    public Korisnik Korisnik { get; set; } = null!;

    public KorisnikAktivnostTip Tip { get; set; }

    [ForeignKey(nameof(Usluga))]
    public int? UslugaId { get; set; }
    public Usluga? Usluga { get; set; }

    public int? KategorijaUslugaId { get; set; }

    [MaxLength(200)]
    public string? SearchTerm { get; set; }
}
