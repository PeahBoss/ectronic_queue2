namespace ElectronicQueue.Application.Interfaces;

public interface IRepository<TEntity> where TEntity : class, IEntity
{
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) => AddRangeAsync([entity], cancellationToken);
    Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default) => RemoveRangeAsync([entity], cancellationToken);
    
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAllAsync(Func<bool, TEntity> predicate, CancellationToken cancellationToken = default);

    Task SaveChanges(CancellationToken cancellationToken = default);
}
