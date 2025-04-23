using ElectronicQueue.Core.Entities;
using ElectronicQueue.Application.Interfaces;
using ElectronicQueue.Core.Interfaces;

namespace ElectronicQueue.Application.Specializations;

public class AddOrUpdateSpecializationCommand(
    IRepository<Specialization> specializationsRepository
) : ICommand<AddOrUpdateSpecializationRequest, BaseResponse>
{
    public async Task<BaseResponse> ExecuteAsync(AddOrUpdateSpecializationRequest request, CancellationToken cancellationToken = default)
    {
        bool isAdding = request.Id is null;

        var allSpecializations = await specializationsRepository.GetAllAsync(cancellationToken);
        Specialization? specialization = null;

        if (isAdding)
        {
            specialization = allSpecializations.FirstOrDefault(s => s.Name == request.Name);
            if (specialization is not null)
                return new(409, $"Specialization '{request.Name}' already exists.");
        }
        else
        {
            specialization = allSpecializations.FirstOrDefault(s => s.Id.Value == request.Id!.Value);
            if (specialization is null)
                return new(404, "Specialization not found.");

            specialization.Name = request.Name;
        }

        if (isAdding)
        {
            specialization = new Specialization
            {
                Name = request.Name,
                Doctors = []
            };

            await specializationsRepository.AddAsync(specialization, cancellationToken);
        }

        await specializationsRepository.SaveChanges(cancellationToken);
        return new(200, "OK");
    }
}
