using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ectronic_queue.Application.Interfaces
{
    public interface ICommand<TRequest, TResponse> where TRequest : class, IRequest
                                                   where TResponse : class, IResponse
    {
        Task<TResponse> ExecuteAsync(TRequest request, CancellationToken cancellationToken = default);
    }
}
