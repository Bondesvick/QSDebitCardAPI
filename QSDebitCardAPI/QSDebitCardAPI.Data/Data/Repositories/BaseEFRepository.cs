using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QSDataUpdateAPI.Data.Repositories;

namespace QSDataUpdateAPI.Data.Data.Repositories
{
    public class BaseEFRepository<T, Tkey> : IRepository<T, Tkey> where T : class
    {
        protected readonly QuickServiceDbContext _dbContext; // Beware. Leaky Abstraction Todo: Figure out a way to refactor leaky abstraction on EfContext

        public BaseEFRepository(QuickServiceDbContext context)
        {
            _dbContext = context;
        }

        public virtual async Task<T> AddItem(T tEntity)
        {
            using var db = new QuickServiceDbContext();
            await db.Set<T>().AddAsync(tEntity);
            return (await db.SaveChangesAsync()) > 0 ? tEntity : null;
        }

        public async Task<IEnumerable<T>> AddItems(IEnumerable<T> tEntities)
        {
            try
            {
                using var db = new QuickServiceDbContext();
                await db.Set<T>().AddRangeAsync(tEntities);
                return (await db.SaveChangesAsync()) > 0 ? tEntities : null;
            }
            catch (Exception exception)
            {
                throw;
            }
        }

        public virtual async Task<T> GetItem(Tkey id)
        {
            try
            {
                using var db = new QuickServiceDbContext();
                return await db.Set<T>().FindAsync(id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> GetItems()
        {
            try
            {
                using var db = new QuickServiceDbContext();
                return await db.Set<T>().ToListAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> GetItems(Func<T, bool> predicate)
        {
            try
            {
                using var db = new QuickServiceDbContext();
                return await Task.Run(() => db.Set<T>().Where(predicate).ToList());
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task<bool> RemoveItem(Tkey id)
        {
            try
            {
                using (var db = new QuickServiceDbContext())
                {
                    var item = await GetItem(id);
                    if (item == null)
                        return false;
                    db.Set<T>().Remove(item);
                    return await db.SaveChangesAsync() > 0;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task<T> UpdateItem(Tkey id, T updatedEntity)
        {
            try
            {
                using (var db = new QuickServiceDbContext())
                {
                    var item = await GetItem(id);
                    if (item == null)
                        throw new KeyNotFoundException($"Item with key {id} not found");
                    db.Entry<T>(updatedEntity).State = EntityState.Modified; // .CurrentValues..SetValues(updatedItem);
                    var updated = await db.SaveChangesAsync() == 1;
                    return updated ? item : null;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}