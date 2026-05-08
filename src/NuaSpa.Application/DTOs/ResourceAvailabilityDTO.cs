using System;
using System.Collections.Generic;

namespace NuaSpa.Application.DTOs;

public class ResourceAvailabilityDTO
{
    public DateTime Slot { get; set; }
    public List<ProstorijaDTO> FreeRooms { get; set; } = new();
    public List<OpremaAvailabilityDTO> Equipment { get; set; } = new();
}

public class OpremaAvailabilityDTO
{
    public int OpremaId { get; set; }
    public string Naziv { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Reserved { get; set; }
    public int Remaining { get; set; }
}

