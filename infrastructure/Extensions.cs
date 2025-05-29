using ElectronicQueue.Infrastructure.Repos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace ElectronicQueue.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddElectronicQueueRepositories(this IServiceCollection services)
    {
        var entityTypes = Assembly.GetAssembly(typeof(IEntity))?
                                .GetTypes()
                                .Where(t => typeof(IEntity).IsAssignableFrom(t)
                                         && t.IsClass && !t.IsAbstract)
                                .ToList();

        if (entityTypes == null || entityTypes.Count == 0)
            throw new InvalidOperationException("Не найдены типы IEntity для регистрации репозиториев.");

        foreach (var entityType in entityTypes)
        {
            var repoInterfaceType = typeof(IRepository<>).MakeGenericType(entityType);
            var repoImplementationType = typeof(BaseRepository<>).MakeGenericType(entityType);

            // Регистрируем репозиторий с внедрением DbContext через конструктор (Scoped)
            services.AddScoped(repoInterfaceType, repoImplementationType);
        }

        return services;
    }

    public static IServiceCollection AddFullElectronicQueue(this IServiceCollection services)
    {
        services.AddElectronicQueueRepositories();

        // Здесь можно добавить регистрацию сервисов бизнес-логики, например:
        // services.AddScoped<IPatientService, PatientService>();
        // services.AddScoped<ISpecializationService, SpecializationService>();

        return services;
    }
}

