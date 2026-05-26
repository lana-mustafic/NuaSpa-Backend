using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Application.Common;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Exceptions;
using NuaSpa.Application.Interfaces;
using NuaSpa.Application.SearchObjects;
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

        public virtual async Task<PagedResult<T>> Get(TSearch? search = null)
        {
            var (page, pageSize) = search is IPagedSearch paged
                ? PaginationHelper.FromSearch(paged)
                : PaginationHelper.Normalize(1, PaginationConstants.DefaultPageSize);

            var query = _context.Set<TDb>().AsNoTracking();
            var pagedEntities = await PaginationHelper.ToPagedAsync(query, page, pageSize);
            return new PagedResult<T>
            {
                Ukupno = pagedEntities.Ukupno,
                Stranica = pagedEntities.Stranica,
                VelicinaStranice = pagedEntities.VelicinaStranice,
                Items = _mapper.Map<IReadOnlyList<T>>(pagedEntities.Items),
            };
        }

        public virtual async Task<T> GetById(int id)
        {
            var entity = await _context.Set<TDb>().FindAsync(id);
            if (entity == null)
            {
                throw new NotFoundException($"{typeof(TDb).Name} id={id} ne postoji.");
            }

            return _mapper.Map<T>(entity);
        }

        public virtual async Task<T> Insert(T dto)
        {
            var set = _context.Set<TDb>();
            TDb entity = _mapper.Map<TDb>(dto);

            set.Add(entity);
            await _context.SaveChangesAsync();

            return _mapper.Map<T>(entity);
        }
    }
}
