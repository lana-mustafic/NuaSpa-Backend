namespace NuaSpa.Domain.Enums;

/// <summary>Životni ciklus rezervacije: Pending → Confirmed → Cancelled | Completed.</summary>
public enum RezervacijaStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Completed = 3,
}
