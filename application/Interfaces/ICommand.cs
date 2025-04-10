namespace ElectronicQueue.Application.Interfaces
{
    public interface ICommand<TRequest, TResponse> where TRequest : class, IRequest
                                                   where TResponse : class, IResponse
    {
        Task<TResponse> ExecuteAsync(TRequest request, CancellationToken cancellationToken = default);
    }
}
