namespace NuaSpa.Application.DTOs;

public class TherapistKpiDTO
{
    public int ZaposlenikId { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }

    public int UkupnoRezervacija { get; set; }
    public int PotvrdjeneRezervacije { get; set; }
    public int OtkazaneRezervacije { get; set; }
    public int PlaceneRezervacije { get; set; }
    public decimal Prihod { get; set; }
    public double ProsjecnaOcjena { get; set; }

    public double StopaOtkazivanjaPostotak { get; set; }
    public int? ZadovoljstvoKlijenataPostotak { get; set; }
    public string Uloga { get; set; } = "Therapist";

    public double? TrendUkupnoRezervacijaPostotak { get; set; }
    public double? TrendPotvrdjenePostotak { get; set; }
    public double? TrendOtkazanePostotak { get; set; }
    public double? TrendProsjecnaOcjenaDelta { get; set; }
    public double? TrendPrihodPostotak { get; set; }
    public double? TrendZadovoljstvoPostotak { get; set; }
}

