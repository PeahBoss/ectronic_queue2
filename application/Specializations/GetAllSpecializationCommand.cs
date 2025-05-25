
namespace ElectronicQueue.Application.Specializations;

public class GetAllSpecializationsCommand(
    IRepository<Specialization> specializationRepository,
    IConverter<Specialization, SpecializationResponse> converter,
    ILogger<GetAllSpecializationsCommand>? logger = null
) : ICommand<EmptyRequest, SpecializationsArrayResponse>
{
    public async Task<SpecializationsArrayResponse> ExecuteAsync(EmptyRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger?.LogInformation("RQST: Getting all specializations");

            var specs = await specializationRepository.GetAllAsync(cancellationToken);
            var converted = specs.Select(converter.ToResponse);

            return new SpecializationsArrayResponse(200, "OK", converted);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "ERR: Failed to get specializations. {ErrorMessage}", ex.Message);
            return new SpecializationsArrayResponse(500, "Internal server error", []);
        }
    }
}
