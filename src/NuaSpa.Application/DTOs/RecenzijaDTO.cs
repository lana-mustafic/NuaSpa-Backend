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
    }
}
