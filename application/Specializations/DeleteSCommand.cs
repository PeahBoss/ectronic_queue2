using Microsoft.Extensions.Logging;
using ElectronicQueue.Core.Entities;
using ElectronicQueue.Application;

namespace ElectronicQueue.Application.Specializations;

public class DeleteSpecializationCommand(
    IRepository<Specialization> specializationsRepository,
    IValidator<Specialization>? specializationValidator = null,
    ILogger<DeleteSpecializationCommand>? logger = null
) : ICommand<ByIdRequest, BaseResponse>
{
    public async Task<BaseResponse> ExecuteAsync(ByIdRequest request,
                                                 CancellationToken cancellationToken = default)
    {
        try
        {
            logger?.LogInformation("RQST: Delete specialization ID {SpecializationId}", request.Id);

            var specialization = await specializationsRepository.GetOneAsync(s => s.Id.Value == request.Id, cancellationToken);
            if (specialization is null)
            {
                logger?.LogWarning("RQST: Specialization ID {SpecializationId} not found", request.Id);
                return new BaseResponse(404, "Specialization not found");
            }

            if (specializationValidator is not null)
            {
                var valid = await specializationValidator.ValidateAsync(specialization, cancellationToken);
                if (!valid)
                {
                    logger?.LogWarning("RQST: Validation failed for specialization ID {SpecializationId}", request.Id);
                    return new BaseResponse(409, "Specialization cannot be deleted due to dependencies");
                }
            }

            await specializationsRepository.RemoveAsync(specialization, cancellationToken);
            logger?.LogInformation("RQST: Specialization ID {SpecializationId} deleted", request.Id);

            return new BaseResponse(200, "OK");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "ERR: Delete specialization ID {SpecializationId} failed: {Message}",
                request.Id, ex.Message);
            return new BaseResponse(500, "Internal server error");
        }
    }
}
