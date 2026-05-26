using NuaSpa.Domain.Enums;

namespace NuaSpa.Application.SearchObjects;

public class ZaposlenikSearchObject
{
    public string? Ime { get; set; }
    public string? Prezime { get; set; }

    /// <summary>Pretraga po imenu, prezimenu ili specijalizaciji.</summary>
    public string? Q { get; set; }

    public int? KategorijaUslugaId { get; set; }
    public ZaposlenikStatus? Status { get; set; }
}
