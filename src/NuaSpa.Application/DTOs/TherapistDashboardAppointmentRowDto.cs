using System;

namespace NuaSpa.Application.DTOs;

/// <summary>Lightweight appointment row for therapist operational dashboard.</summary>
public class TherapistDashboardAppointmentRowDto
{
    public int Id { get; set; }
    public DateTime DatumRezervacije { get; set; }
     public string Status { get; set; } = "Pending";
    public bool IsPotvrdjena { get; set; }
    public bool IsOtkazana { get; set; }
    public string? KorisnikIme { get; set; }
    public string? UslugaNaziv { get; set; }
    public int UslugaTrajanjeMinuta { get; set; }
    public string? NapomenaZaTerapeuta { get; set; }
    public string? ProstorijaNaziv { get; set; }
}
