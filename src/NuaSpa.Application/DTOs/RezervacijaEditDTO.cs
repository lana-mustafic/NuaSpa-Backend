using System;
using System.Collections.Generic;

namespace NuaSpa.Application.DTOs;

public class RezervacijaEditDTO
{
    public DateTime DatumRezervacije { get; set; }
    public int ZaposlenikId { get; set; }
    public int UslugaId { get; set; }
    public int? ProstorijaId { get; set; }

    /// <summary>VIP tretman (admin).</summary>
    public bool IsVip { get; set; }

    public List<RezervacijaOpremaItemDTO>? Oprema { get; set; }
}

