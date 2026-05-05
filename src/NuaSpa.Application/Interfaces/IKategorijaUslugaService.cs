using System.Threading.Tasks;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Interfaces;

public interface IKategorijaUslugaService : IBaseService<KategorijaUslugaDTO, object>
{
    Task<KategorijaUslugaDTO> UpdateAsync(KategorijaUslugaDTO dto);
    Task<(bool Ok, string? Message)> DeleteAsync(int id);
}