using System;
using System.Collections.Generic;
using NuaSpa.Domain.Common;
using System.Text;

namespace NuaSpa.Domain.Entities
{
    public class Grad: BaseEntity
    {
        public string Naziv { get; set; } = null!;
        public string PostanskiBroj { get; set; } = null!;

        // Relacija sa drzavom
        public int DrzavaId { get; set; }
        public Drzava Drzava { get; set; } = null!;
    }
}
