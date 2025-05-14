namespace ElectronicQueue.Tests.Application;

public class FakeRepository<TEntity>(TEntity[]? entities = null) : IRepository<TEntity> where TEntity : class, IEntity
{
    public List<TEntity> Db { get; init; } = [.. entities ?? []];

    public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) =>
        Task.Run(() =>
        {
            foreach (var entity in entities)
            {
                if (entity.Id is null)
                    entity.Id = new(Guid.NewGuid());

                Db.Add(entity);
            }
        }, cancellationToken);

    public Task AddOrUpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) =>
        Task.Run(() =>
        {
            var toAdd = entities.Where(x => x.Id is null).ToArray();
            foreach (var entity in toAdd)
                entity.Id = new(Guid.NewGuid());

            Db.AddRange(toAdd);
        }, cancellationToken);

    public Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) =>
        Task.Run(() => Db.RemoveAll(x => entities.Contains(x)), cancellationToken);

    public Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IEnumerable<TEntity>>(Db);

    public Task<IEnumerable<TEntity>> GetAllAsync(Func<TEntity, bool> predicate, CancellationToken cancellationToken = default) =>
        Task.FromResult(Db.Where(predicate));

    public Task<TEntity?> GetOneAsync(Func<TEntity, bool> predicate, CancellationToken cancellationToken = default) =>
        Task.FromResult(Db.FirstOrDefault(predicate));

    public Task SaveChanges(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
