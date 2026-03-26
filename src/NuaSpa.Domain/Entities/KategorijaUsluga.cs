using System;
using System.Collections.Generic;
using System.Text;
using NuaSpa.Domain.Common;

namespace NuaSpa.Domain.Entities
{
    public class KategorijaUsluga : BaseEntity
    {
        public string Naziv { get; set; } = null!;
        public string? Opis { get; set; }
    }
}
