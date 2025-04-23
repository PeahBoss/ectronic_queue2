namespace ElectronicQueue.Application.Interfaces;

public interface IRepository<TEntity> where TEntity : class, IEntity
{
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => AddRangeAsync([entity], cancellationToken);

    Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
        => RemoveRangeAsync([entity], cancellationToken);

    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAllAsync(Func<TEntity, bool> predicate, CancellationToken cancellationToken = default);

    Task SaveChanges(CancellationToken cancellationToken = default);

    //Добавленные методы
    async Task<TEntity?> GetOneAsync(Func<TEntity, bool> predicate, CancellationToken cancellationToken = default)
        => (await GetAllAsync(predicate, cancellationToken)).ToArray() switch
        {
            { Length: 1 } entities => entities[0],
            _ => null
        };

    Task AddOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        => AddOrUpdateRangeAsync([entity], cancellationToken);

    Task AddOrUpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
}
