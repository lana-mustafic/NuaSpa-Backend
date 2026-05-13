using System.ComponentModel.DataAnnotations;

namespace NuaSpa.Application.DTOs;

public class AdminKlijentUpdateDto
{
    public bool? IsVipKlijent { get; set; }

    public int? ZaposlenikId { get; set; }

    [MaxLength(1200)]
    public string? NapomenaZaTerapeuta { get; set; }
}
