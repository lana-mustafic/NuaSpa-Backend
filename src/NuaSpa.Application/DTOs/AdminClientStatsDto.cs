namespace NuaSpa.Application.DTOs;

/// <summary>
/// Agregati za admin Clients dashboard (isti filteri kao lista ako su proslijeđeni).
/// </summary>
public class AdminClientStatsDto
{
    public int UkupnoKlijenata { get; set; }
    public int VipKlijenata { get; set; }
    public int UkupnoPosjeta { get; set; }
    public decimal UkupnaPotrosnja { get; set; }
}
