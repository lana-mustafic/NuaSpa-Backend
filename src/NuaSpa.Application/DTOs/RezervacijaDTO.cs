using System;
using System.Collections.Generic;
using System.Text;

namespace NuaSpa.Application.DTOs
{
    public class RezervacijaDTO
    {
        public int Id { get; set; }
        public DateTime DatumRezervacije { get; set; }
        public bool IsPotvrdjena { get; set; }
        public string? KorisnikIme { get; set; }
        public string? UslugaNaziv { get; set; }
        public string? ZaposlenikIme { get; set; }
    }
}
