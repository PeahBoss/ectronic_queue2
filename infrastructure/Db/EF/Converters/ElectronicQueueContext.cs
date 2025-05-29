using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicQueue.Infrastructure.Db.EF.Converters
{
    internal class ElectronicQueueContext(DbContextOptions options) : DbContext(options)
    {
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<Id>()
                                .HaveConversion<IdConverter>();

            base.ConfigureConventions(configurationBuilder);
        }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            var entities = typeof(IEntity).Assembly
                                          .GetTypes()
                                          .Where(t => typeof(IEntity).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                                          .ToArray();

            foreach (var entityType in entities)
            {
                mb.Entity(entityType).HasKey(nameof(IEntity.Id));
                mb.Entity(entityType).Property(nameof(IEntity.Id)).HasDefaultValueSql("NEWSEQUENTIALID()"); // для T-SQL
            }

            base.OnModelCreating(mb);
        }

        // костыль если провайдер БД не умеет в генерацию айдишников, а очень хочется переложить ответственность на БД 
        // public override EntityEntry Add(object entity)
        // {
        //     if (entity is IEntity e)
        //         e.Id = new(Guid.NewGuid());
        //     return base.Add(entity);
        // }
    }
}
