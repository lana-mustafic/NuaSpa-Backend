using System;
using System.Collections.Generic;
using System.Text;

namespace NuaSpa.Application.Interfaces
{
    public interface IBaseService<T, TSearch> where T : class where TSearch : class
    {
        Task<IEnumerable<T>> Get(TSearch? search = null);
        Task<T> GetById(int id);
        Task<T> Insert(T dto);
    }
}