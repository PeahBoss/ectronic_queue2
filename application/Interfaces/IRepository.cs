namespace ElectronicQueue.Application.Interfaces
{
    public interface IRepository<TEntity> where TEntity : class, IEntity
    {
        // Синхронный метод для добавления одной сущности
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
            => AddRangeAsync(new[] { entity }, cancellationToken);

        // Синхронный метод для удаления одной сущности
        Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
            => RemoveRangeAsync(new[] { entity }, cancellationToken);

        // Асинхронный метод для добавления нескольких сущностей
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        // Асинхронный метод для удаления нескольких сущностей
        Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        // Асинхронный метод для получения всех сущностей
        Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

        // Асинхронный метод для получения сущностей по предикату
        Task<IEnumerable<TEntity>> GetAllAsync(Func<TEntity, bool> predicate, CancellationToken cancellationToken = default);

        // Асинхронный метод для сохранения изменений в контексте
        Task SaveChangesAsync(CancellationToken cancellationToken = default);

        // Метод для получения одной сущности по предикату
        async Task<TEntity?> GetOneAsync(Func<TEntity, bool> predicate, CancellationToken cancellationToken = default)
            => (await GetAllAsync(predicate, cancellationToken)).ToArray() switch
            {
                { Length: 1 } entities => entities[0],
                _ => null
            };

        // Метод для добавления или обновления одной сущности
        Task AddOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
            => AddOrUpdateRangeAsync(new[] { entity }, cancellationToken);

        // Метод для добавления или обновления нескольких сущностей
        Task AddOrUpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
        
    }
}
