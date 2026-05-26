using System.Threading.Tasks;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Interfaces
{
    public interface IBaseService<T, TSearch> where T : class where TSearch : class
    {
        Task<PagedResult<T>> Get(TSearch? search = null);
        Task<T> GetById(int id);
        Task<T> Insert(T dto);
    }
}
