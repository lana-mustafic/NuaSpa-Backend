namespace NuaSpa.Application.DTOs;

public class RevenuePointDTO
{
    public DateTime Datum { get; set; }

    /// <summary>Svi termini zakazani za taj dan (bez obrisanih).</summary>
    public int BrojRezervacija { get; set; }

    /// <summary>Broj uspješnih uplata za taj dan (DatumPlacanja).</summary>
    public int BrojPlacanja { get; set; }

    /// <summary>Potvrđeni i nisu otkazani.</summary>
    public int BrojPotvrdjenih { get; set; }

    /// <summary>Otkazani termini za taj dan.</summary>
    public int BrojOtkazanih { get; set; }

    public decimal Prihod { get; set; }
}

