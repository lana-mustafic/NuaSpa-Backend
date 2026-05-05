using System.Collections.Generic;
using System.Threading.Tasks;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Interfaces
{
    public interface IRecenzijaService
    {
        Task<IEnumerable<RecenzijaDTO>> GetByUslugaAsync(int uslugaId);

        Task<RecenzijaDTO> CreateAsync(int korisnikId, RecenzijaCreateDTO dto);
    }
}

