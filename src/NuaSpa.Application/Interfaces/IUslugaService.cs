using NuaSpa.Application.DTOs;
using NuaSpa.Application.SearchObjects;

namespace NuaSpa.Application.Interfaces
{
    public interface IUslugaService : IBaseService<UslugaDTO, UslugaSearchObject>
    {
        Task<UslugaDTO> UpdateAsync(UslugaDTO dto);
        Task<(bool Ok, string? Message)> DeleteAsync(int id);
    }
}