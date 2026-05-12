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
}
