namespace NuaSpa.Application.Interfaces;

public interface IBaseService<TModel, TSearch>
    where TModel : class
    where TSearch : class
{
    Task<IEnumerable<TModel>> Get(TSearch? search = null);
    Task<TModel> GetById(int id);
}