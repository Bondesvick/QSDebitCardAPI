using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QSDataUpdateAPI.Data.Repositories
{
    public interface IRepository<T, Tkey> where T : class
    {
        Task<T> AddItem(T tEntity);
        Task<IEnumerable<T>> AddItems(IEnumerable<T> tEntities);
        Task<T> GetItem(Tkey id);
        Task<bool> RemoveItem(Tkey id);
        Task<T> UpdateItem(Tkey id, T updatedEntity);
        Task<IEnumerable<T>> GetItems();
        Task<IEnumerable<T>> GetItems(Func<T, bool> predicate);
    }
}
