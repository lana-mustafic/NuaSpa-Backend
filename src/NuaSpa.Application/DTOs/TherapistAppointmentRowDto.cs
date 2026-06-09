using System;

namespace NuaSpa.Application.DTOs;

/// <summary>Operational appointment row for therapist My Appointments list.</summary>
public class TherapistAppointmentRowDto
{
    public int Id { get; set; }
    public DateTime DatumRezervacije { get; set; }
    public string Status { get; set; } = "Pending";
    public bool IsPotvrdjena { get; set; }
    public bool IsPlacena { get; set; }
    public bool IsOtkazana { get; set; }
    public string? RazlogOtkaza { get; set; }
    public DateTime? OtkazanaAt { get; set; }
    public bool IsVip { get; set; }
    public bool PremiumKlijent { get; set; }

    public int KorisnikId { get; set; }
    public string? KorisnikIme { get; set; }
    public string? KorisnikTelefon { get; set; }
    public string? KorisnikEmail { get; set; }
    public string? NapomenaZaTerapeuta { get; set; }

    public string? UslugaNaziv { get; set; }
    public int UslugaId { get; set; }
    public int UslugaTrajanjeMinuta { get; set; }
    public decimal UslugaCijena { get; set; }

    public string? ProstorijaNaziv { get; set; }
}
