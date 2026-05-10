namespace NuaSpa.Application.DTOs;

/// <summary>
/// Agregat za desktop Home (sve uloge; neka polja su null kad korisnik nema pravo).
/// </summary>
public class DesktopHomeOverviewDto
{
    /// <summary>Broj novih registracija u zadnjih 7 dana (samo Admin).</summary>
    public int? NoviKlijentiZadnjih7Dana { get; set; }

    /// <summary>
    /// Suma cijena usluga za aktivne (neotkazane) rezervacije za odabrani dan,
    /// ograničeno na ulogu (klijent / terapeut / cijeli spa za admina).
    /// </summary>
    public decimal ProcijenjeniPrihodZaDan { get; set; }

    public string Valuta { get; set; } = "KM";
}
