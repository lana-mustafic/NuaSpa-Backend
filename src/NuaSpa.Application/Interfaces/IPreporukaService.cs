using System.Collections.Generic;
using System.Threading.Tasks;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Interfaces
{
    public interface IPreporukaService
    {
        Task LogAktivnostAsync(int korisnikId, KorisnikAktivnostCreateDto dto);

        Task<IEnumerable<PreporucenaUslugaDto>> GetPreporukeAsync(int korisnikId, int take = 10);
    }
}
