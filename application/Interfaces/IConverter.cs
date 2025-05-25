namespace ElectronicQueue.Application.Interfaces
{
    public interface IConverter<TSource, TDestination>
    {
        TDestination ToResponse(TSource source);
    }
}
