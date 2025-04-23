
namespace ElectronicQueue.Application.Doctors;

public class AddOrUpdateDoctorCommand(
    IRepository<Doctor> doctorsRepository,
    IRepository<Specialization> specializationsRepository
) : ICommand<AddOrUpdateDoctorRequest, BaseResponse>
{
    public async Task<BaseResponse> ExecuteAsync(AddOrUpdateDoctorRequest request, CancellationToken cancellationToken = default)
    {
        bool isAdding = request.Id is null;

        // Получаем всех врачей и фильтруем в памяти
        var allDoctors = await doctorsRepository.GetAllAsync(cancellationToken);
        Doctor? doctor = null;

        if (isAdding)
        {
            doctor = allDoctors.FirstOrDefault(d =>
                d.Name == request.Name && d.PhoneNumber == request.PhoneNumber);

            if (doctor is not null)
                return new(409, $"Doctor with phone number {request.PhoneNumber} already exists.");
        }
        else
        {
            doctor = allDoctors.FirstOrDefault(d => d.Id.Value == request.Id!.Value);
            if (doctor is null)
                return new(404, "Doctor not found.");

            doctor.Name = request.Name;
            doctor.PhoneNumber = request.PhoneNumber;
            doctor.OfficeNumber = request.OfficeNumber;
            doctor.WorkSchedule = request.WorkSchedule;
        }

        // Обработка специализаций
        if (request.SpecializationIds.Any())
        {
            var allSpecializations = await specializationsRepository.GetAllAsync(cancellationToken);
            var matchedSpecializations = allSpecializations
                .Where(s => request.SpecializationIds.Contains(s.Id.Value))
                .ToList();

            if (matchedSpecializations.Count != request.SpecializationIds.Count)
                return new(404, "One or more specializations not found.");

            if (doctor is not null)
                doctor.Specializations = matchedSpecializations;
        }

        if (isAdding)
        {
            doctor = new Doctor
            {
                Name = request.Name,
                PhoneNumber = request.PhoneNumber,
                OfficeNumber = request.OfficeNumber,
                WorkSchedule = request.WorkSchedule,
                Specializations = request.SpecializationIds.Any()
                    ? new List<Specialization>() // будет заменено выше
                    : new List<Specialization>()
            };

            await doctorsRepository.AddAsync(doctor, cancellationToken);
        }

        await doctorsRepository.SaveChanges(cancellationToken);
        return new(200, "OK");
    }
}
