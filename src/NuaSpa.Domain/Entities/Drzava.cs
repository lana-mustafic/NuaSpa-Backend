using System;
using System.Collections.Generic;
using System.Text;
using NuaSpa.Domain.Common;

namespace NuaSpa.Domain.Entities
{
    public class Drzava : BaseEntity
    {
        public string Naziv { get; set; } = null!;
        public string PozivniBroj { get; set; } = null!;
    }
}
