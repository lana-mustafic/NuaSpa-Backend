using NuaSpa.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace NuaSpa.Domain.Entities
{
    public class Zaposlenik : BaseEntity
    {
        public string Ime { get; set; } = null!;
        public string Prezime { get; set; } = null!;
        public string Specijalizacija { get; set; } = null!;
        public DateTime DatumZaposlenja { get; set; }
    }
}
