namespace ElectronicQueue.Infrastructure.Repos
{
    public class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity
    {
#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
        private BaseRepository() { }
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
        private ElectronicQueueContext DbContext { get; init; }

        public static BaseRepository<TEntity> CreateRepository(DbContextOptions dbContextOptions) =>
            new() { DbContext = new(dbContextOptions) };

        public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
            => await AddRangeAsync(new[] { entity }, cancellationToken);

        public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            await DbContext.AddRangeAsync(entities, cancellationToken); // Добавление нескольких сущностей
            await DbContext.SaveChangesAsync(cancellationToken); // Сохранение изменений в базе данных
        }

        public async Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
            => await RemoveRangeAsync(new[] { entity }, cancellationToken);

        public async Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            DbContext.RemoveRange(entities); // Удаление нескольких сущностей
            await DbContext.SaveChangesAsync(cancellationToken); // Сохранение изменений в базе данных
        }

        public Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(DbContext.Set<TEntity>().ToArray().AsEnumerable());

        public Task<IEnumerable<TEntity>> GetAllAsync(Func<TEntity, bool> predicate, CancellationToken cancellationToken = default) =>
            Task.FromResult(DbContext.Set<TEntity>().Where(predicate).ToArray().AsEnumerable());

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
            await DbContext.SaveChangesAsync(cancellationToken);

        public async Task<TEntity?> GetOneAsync(Func<TEntity, bool> predicate, CancellationToken cancellationToken = default)
            => (await GetAllAsync(predicate, cancellationToken)).ToArray() switch
            {
                { Length: 1 } entities => entities[0],
                _ => null
            };

        public async Task AddOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
            => await AddOrUpdateRangeAsync(new[] { entity }, cancellationToken);

        public async Task AddOrUpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            var toAdd = entities.Where(e => e.Id is null).ToArray();
            var toUpdate = entities.Except(toAdd).ToArray();

            DbContext.UpdateRange(toUpdate); // Обновление сущностей
            await DbContext.AddRangeAsync(toAdd, cancellationToken); // Добавление новых сущностей
            await DbContext.SaveChangesAsync(cancellationToken); // Сохранение изменений в базе данных
        }
    }
}
