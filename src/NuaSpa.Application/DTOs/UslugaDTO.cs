using System;
using System.Collections.Generic;
using System.Text;

namespace NuaSpa.Application.DTOs
{
    public class UslugaDTO
    {
        public int Id { get; set; }
        public string Naziv { get; set; } = null!;
        public decimal Cijena { get; set; }
        public int TrajanjeMinuta { get; set; }
    }
}
