namespace ElectronicQueue.Application.Patients;

public class AddOrUpdatePatientCommand(
    IRepository<Patient> patientsRepository
) : ICommand<AddOrUpdatePatientRequest, BaseResponse>
{
    public async Task<BaseResponse> ExecuteAsync(AddOrUpdatePatientRequest request, CancellationToken cancellationToken = default)
    {
        bool isAdding = request.Id is null;

        // Получаем всех пациентов и фильтруем в памяти
        var allPatients = await patientsRepository.GetAllAsync(cancellationToken);
        Patient patient = null!;

        if (isAdding)
        {
            var potentialPatient = allPatients.FirstOrDefault(p =>
                p.Name == request.Name && p.PhoneNumber == request.PhoneNumber);

            if (potentialPatient is not null)
                return new(409, $"Patient with phone number {request.PhoneNumber} already exists.");

            patient = new Patient
            {
                Appointments = []
            };
        }
        else
        {
            var potentialPatient = allPatients.FirstOrDefault(p => p.Id.Value == request.Id!.Value);
            if (potentialPatient is null)
                return new(404, "Patient not found.");
            else
                patient = potentialPatient;
        }

        // Обновляем или присваиваем поля
        patient.Name = request.Name;
        patient.Birthday = request.Birthday;
        patient.Gender = request.Gender;
        patient.PhoneNumber = request.PhoneNumber;
        patient.InsuranceNumber = request.InsuranceNumber;

        if (isAdding)
        {
            await patientsRepository.AddAsync(patient, cancellationToken);
        }

        await patientsRepository.SaveChanges(cancellationToken);
        return new(200, "OK");
    }
}

