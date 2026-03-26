using NuaSpa.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace NuaSpa.Domain.Entities
{
    public class Uloga : BaseEntity
    {
        public string Naziv { get; set; } = null!;
    }
}
