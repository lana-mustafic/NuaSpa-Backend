using System;

namespace NuaSpa.Application.DTOs
{
    public class RezervacijaCreateDTO
    {
        public DateTime DatumRezervacije { get; set; }

        public int UslugaId { get; set; }

        public int ZaposlenikId { get; set; }
    }
}

