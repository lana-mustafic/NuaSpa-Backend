using System;
using System.Collections.Generic;

namespace NuaSpa.Application.DTOs;

/// <summary>
/// Polja koja klijent smije poslati pri kreiranju rezervacije:
/// usluga, terapeut, datum i termin.
/// </summary>
public class RezervacijaClientCreateDTO
{
    public DateTime DatumRezervacije { get; set; }

    public int UslugaId { get; set; }

    public int ZaposlenikId { get; set; }
}

/// <summary>
/// Administratorsko kreiranje. Prostorija, oprema, VIP i KorisnikId su interni podaci.
/// Klijentski poziv ove vrijednosti ignorira servis.
/// </summary>
public class RezervacijaCreateDTO : RezervacijaClientCreateDTO
{
    public int? KorisnikId { get; set; }

    public int? ProstorijaId { get; set; }

    /// <summary>VIP tretman (admin); default false.</summary>
    public bool IsVip { get; set; }

    public List<RezervacijaOpremaItemDTO>? Oprema { get; set; }
}
