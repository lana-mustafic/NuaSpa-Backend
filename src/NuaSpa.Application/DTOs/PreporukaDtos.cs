namespace NuaSpa.Application.DTOs;

public static class PreporukaRazlogKod
{
    public const string PastBookingCategory = "PAST_BOOKING_CATEGORY";
    public const string FavoriteCategory = "FAVORITE_CATEGORY";
    public const string SearchInterest = "SEARCH_INTEREST";
    public const string ViewedSimilar = "VIEWED_SIMILAR";
    public const string Popular = "POPULAR";
    public const string NewInCategory = "NEW_IN_CATEGORY";
}

public class KorisnikAktivnostCreateDto
{
    /// <summary>0 = Search, 1 = ViewService</summary>
    public int Tip { get; set; }

    public int? UslugaId { get; set; }
    public int? KategorijaUslugaId { get; set; }
    public string? SearchTerm { get; set; }
}

public class PreporucenaUslugaDto
{
    public UslugaDTO Usluga { get; set; } = null!;
    public string RazlogKod { get; set; } = null!;
    public string RazlogTekst { get; set; } = null!;
    public double Skor { get; set; }
}
