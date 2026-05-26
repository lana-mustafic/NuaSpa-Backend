using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Interfaces
{
    public interface ILookupService
    {
        Task<List<DrzavaLookupDto>> GetDrzaveAsync(string? naziv, CancellationToken ct);
        Task<List<GradLookupDto>> GetGradoviAsync(int? drzavaId, string? naziv, CancellationToken ct);
    }
}

