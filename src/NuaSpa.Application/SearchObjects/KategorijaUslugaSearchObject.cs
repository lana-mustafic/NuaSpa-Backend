namespace NuaSpa.Application.SearchObjects;

public class KategorijaUslugaSearchObject
{
    public string? Naziv { get; set; }

    /// <summary>Uključi obrisane zapise (admin).</summary>
    public bool IncludeDeleted { get; set; }
}
