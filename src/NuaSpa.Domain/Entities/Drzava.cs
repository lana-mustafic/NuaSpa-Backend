using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations; // OBAVEZNO DODAJ OVO
using NuaSpa.Domain.Common;

namespace NuaSpa.Domain.Entities
{
    public class Drzava : BaseEntity
    {
        [Required] 
        [MaxLength(100)] 
        public string Naziv { get; set; } = null!;

        [Required] 
        [MaxLength(10)] 
        public string PozivniBroj { get; set; } = null!;
    }
}