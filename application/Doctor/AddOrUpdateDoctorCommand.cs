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
        Doctor doctor = null!;

        if (isAdding)
        {
            var potentialDoctor = allDoctors.FirstOrDefault(d =>
                d.Name == request.Name && d.PhoneNumber == request.PhoneNumber);

            if (potentialDoctor is not null)
                return new(409, $"Doctor with phone number {request.PhoneNumber} already exists.");
            doctor = new();
        }
        else
        {
            var potentialDoctor = allDoctors.FirstOrDefault(d => d.Id.Value == request.Id!.Value);
            if (potentialDoctor is null)
                return new(404, "Doctor not found.");
            else 
                doctor = potentialDoctor;

        }
        doctor.Name = request.Name;
        doctor.PhoneNumber = request.PhoneNumber;
        doctor.OfficeNumber = request.OfficeNumber;
        doctor.WorkSchedule = request.WorkSchedule;

        // Обработка специализаций
        var allSpecializations = await specializationsRepository.GetAllAsync(cancellationToken);
        var matchedSpecializations = allSpecializations
            .Where(s => request.SpecializationIds.Contains(s.Id.Value))
            .ToList();

        if (matchedSpecializations.Count != request.SpecializationIds.Length)
            return new(404, "One or more specializations not found.");

        doctor.Specializations = [.. matchedSpecializations];


        await doctorsRepository.AddOrUpdateAsync(doctor, cancellationToken);
        await doctorsRepository.SaveChanges(cancellationToken);
        return new(200, "OK");
    }
}
