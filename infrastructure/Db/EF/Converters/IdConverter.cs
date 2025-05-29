
namespace ElectronicQueue.Infrastructure.Db.EF.Converters
{
    internal class IdConverter() : ValueConverter<Id, Guid>(v => v.Value, v => new Id(v));
}
