namespace NuaSpa.Application.DTOs;

/// <summary>Limited fields a therapist may update on their own profile.</summary>
public class TherapistSelfProfileUpdateDto
{
    public string? Telefon { get; set; }
    public string? Jezici { get; set; }
}
