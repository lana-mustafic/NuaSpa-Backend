using System;

namespace NuaSpa.Application.DTOs
{
    public class RezervacijaCreateDTO
    {
        public int? KorisnikId { get; set; }

        public DateTime DatumRezervacije { get; set; }

        public int UslugaId { get; set; }

        public int ZaposlenikId { get; set; }

        public int? ProstorijaId { get; set; }

        public List<RezervacijaOpremaItemDTO>? Oprema { get; set; }
    }
}

