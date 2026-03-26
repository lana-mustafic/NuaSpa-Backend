using NuaSpa.Domain.Common;

namespace NuaSpa.Domain.Entities;

public class Popust : BaseEntity
{
    public string Naziv { get; set; } = null!; // npr. "Ramazanski popust"
    public decimal Procenat { get; set; } // npr. 20.00
    public DateTime VrijediOd { get; set; }
    public DateTime VrijediDo { get; set; }
    public bool IsAktivan { get; set; } = true;
}