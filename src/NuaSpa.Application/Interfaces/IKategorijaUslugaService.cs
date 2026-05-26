using System.Threading.Tasks;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.SearchObjects;

namespace NuaSpa.Application.Interfaces;

public interface IKategorijaUslugaService : IBaseService<KategorijaUslugaDTO, KategorijaUslugaSearchObject>
{
    Task<KategorijaUslugaDTO> UpdateAsync(KategorijaUslugaDTO dto);
    Task<(bool Ok, string? Message)> DeleteAsync(int id);
}