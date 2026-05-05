using System.Collections.Generic;
using System.Threading.Tasks;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Interfaces
{
    public interface IPreporukaService
    {
        Task<IEnumerable<UslugaDTO>> GetForKorisnikAsync(int korisnikId, int take = 10);
    }
}
