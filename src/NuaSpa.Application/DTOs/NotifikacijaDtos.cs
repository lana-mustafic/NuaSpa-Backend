using System;
using System.Collections.Generic;
using NuaSpa.Domain.Enums;

namespace NuaSpa.Application.DTOs;

public class SistemskaNotifikacijaDto
{
    public int Id { get; set; }
    public SistemskaNotifikacijaTip Tip { get; set; }
    public string Naslov { get; set; } = string.Empty;
    public string Tekst { get; set; } = string.Empty;
    public bool Procitana { get; set; }
    public DateTime DatumVrijeme { get; set; }
    public int? RezervacijaId { get; set; }
}

public class SistemskaNotifikacijaUnreadDto
{
    public int BrojNeprocitanih { get; set; }
}

public class ObavijestDto
{
    public int Id { get; set; }
    public string Naslov { get; set; } = string.Empty;
    public string Tekst { get; set; } = string.Empty;
    public string? SlikaUrl { get; set; }
    public DateTime DatumObjave { get; set; }
    public bool Aktivna { get; set; }
}

public class ObavijestCreateDto
{
    public string Naslov { get; set; } = string.Empty;
    public string Tekst { get; set; } = string.Empty;
    public string? SlikaUrl { get; set; }
    public DateTime? DatumObjave { get; set; }
    public bool Aktivna { get; set; } = true;
}

public class ObavijestUpdateDto : ObavijestCreateDto
{
}
