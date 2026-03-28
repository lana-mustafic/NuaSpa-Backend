using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Application.Interfaces;
using NuaSpa.Domain;

namespace NuaSpa.Application.Services
{
    public class BaseService<T, TDb, TSearch> : IBaseService<T, TSearch>
        where T : class where TDb : class where TSearch : class
    {
        protected readonly NuaSpaContext _context;
        protected readonly IMapper _mapper;

        public BaseService(NuaSpaContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public virtual async Task<IEnumerable<T>> Get(TSearch? search = null)
        {
            var entity = _context.Set<TDb>();
            var list = await entity.ToListAsync();
            return _mapper.Map<IEnumerable<T>>(list);
        }

        public virtual async Task<T> GetById(int id)
        {
            var entity = await _context.Set<TDb>().FindAsync(id);
            return _mapper.Map<T>(entity);
        }
    }
}