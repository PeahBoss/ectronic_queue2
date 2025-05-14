using ElectronicQueue.Application;

namespace ElectronicQueue.Tests.Application;

public class Invalidator<TEntity> : IValidator<TEntity> where TEntity : class, IEntity
{
    public Task<bool> ValidateAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        Task.FromResult(false);
}
