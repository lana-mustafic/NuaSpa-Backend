using System;
using System.Collections.Generic;
using System.Text;

namespace NuaSpa.Application.DTOs
{
    public class RecenzijaDTO
    {
        public int Id { get; set; }
        public int Ocjena { get; set; }
        public string? Komentar { get; set; }
        public string? KorisnikIme { get; set; }
        public string? UslugaNaziv { get; set; }
        public string? ZaposlenikIme { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? AdminOdgovor { get; set; }
    }
}
