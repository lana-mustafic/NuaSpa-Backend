using System.Collections.Generic;

namespace NuaSpa.Application.DTOs;

public class PagedResult<T>
{
    public int Ukupno { get; set; }
    public int Stranica { get; set; }
    public int VelicinaStranice { get; set; }
    public IReadOnlyList<T> Items { get; set; } = new List<T>();
}
