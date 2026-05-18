using System.Collections.Generic;

namespace NuaSpa.Application.DTOs;

public class TherapistAdminProfileDto
{
    public ZaposlenikDTO Terapeut { get; set; } = null!;
    public string? PovezanEmail { get; set; }
    public bool ImaKorisnickiNalog { get; set; }
    /// <summary>Napomena s kartice povezanog korisničkog računa terapeuta (vidljivo adminu/terapeutu).</summary>
    public string? InternaNapomena { get; set; }
    public IReadOnlyList<TherapistReviewRowDto> NedavneRecenzije { get; set; } =
        new List<TherapistReviewRowDto>();

    public string? LokacijaPrikaz { get; set; }
    public string Uloga { get; set; } = "Therapist";
    public TherapistKpiDTO? Kpi { get; set; }
    public IReadOnlyList<TherapistWeeklyScheduleDayDto> SedmicniRaspored { get; set; } =
        new List<TherapistWeeklyScheduleDayDto>();
    public IReadOnlyList<TherapistTopServiceDto> TopUsluge { get; set; } =
        new List<TherapistTopServiceDto>();
}
