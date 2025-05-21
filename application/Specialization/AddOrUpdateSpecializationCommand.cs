using Microsoft.Extensions.Logging;

namespace ElectronicQueue.Application.Specializations;

public class AddOrUpdateSpecializationCommand(
    IRepository<Specialization> specializationsRepository,
    IValidator<Specialization>? specializationValidator = null,
    ILogger<AddOrUpdateSpecializationCommand>? logger = null
) : ICommand<AddOrUpdateSpecializationRequest, BaseResponse>
{
    public async Task<BaseResponse> ExecuteAsync(AddOrUpdateSpecializationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            bool isAdding = request.Id is null;
            logger?.LogInformation("RQST: {Operation} specialization '{SpecializationName}'",
                                   isAdding ? "creation" : "update",
                                   request.Name);

            var allSpecializations = await specializationsRepository.GetAllAsync(cancellationToken);
            Specialization specialization;

            if (isAdding)
            {
                var potentialSpecialization = allSpecializations.FirstOrDefault(s => s.Name == request.Name);
                if (potentialSpecialization is not null)
                {
                    logger?.LogWarning("RQST: failed: Specialization '{Name}' already exists", request.Name);
                    return new(409, $"Specialization '{request.Name}' already exists.");
                }

                specialization = new Specialization
                {
                    Name = request.Name,
                    Doctors = []
                };
            }
            else
            {
                specialization = allSpecializations.FirstOrDefault(s => s.Id.Value == request.Id!.Value)!;

                if (specialization is null)
                {
                    logger?.LogWarning("RQST: failed: Specialization ID {SpecializationId} not found", request.Id);
                    return new(404, "Specialization not found.");
                }
            }

            var oldName = specialization.Name;
            specialization.Name = request.Name;

            if (specializationValidator is not null && !await specializationValidator.ValidateAsync(specialization, cancellationToken))
            {
                logger?.LogWarning("RQST: failed: validation failed for specialization '{SpecializationName}'", request.Name);
                specialization.Name = oldName;
                return new(400, "Specialization validation failed");
            }

            await specializationsRepository.AddOrUpdateAsync(specialization, cancellationToken);
            await specializationsRepository.SaveChanges(cancellationToken);

            return new(200, "OK");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "RQST: failed: unexpected error during {Operation} for specialization '{SpecializationName}'",
                             request.Id is null ? "creation" : "update", request.Name);
            return new(500, "An error occurred while processing your request");
        }
    }
}
