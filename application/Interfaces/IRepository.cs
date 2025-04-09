using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ectronic_queue.Application.Interfaces
{
    public interface IRepository<TEntity> where TEntity : class, IEntity
    {
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) => AddRangeAsync([entity], cancellationToken);
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
        Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default) => RemoveRangeAsync([entity], cancellationToken);
        Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> GetAllAsync(Func<bool, TEntity> predicate, CancellationToken cancellationToken = default);

        Task SaveChanges();
    }
}
