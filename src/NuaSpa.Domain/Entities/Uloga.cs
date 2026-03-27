using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using NuaSpa.Domain.Common;

namespace NuaSpa.Domain.Entities
{
    public class Uloga : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Naziv { get; set; } = null!;
    }
}