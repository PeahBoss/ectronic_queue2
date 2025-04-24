namespace ElectronicQueue.Application.Patients;

public class AddOrUpdatePatientCommand(
    IRepository<Patient> patientsRepository
) : ICommand<AddOrUpdatePatientRequest, BaseResponse>
{
    public async Task<BaseResponse> ExecuteAsync(AddOrUpdatePatientRequest request, CancellationToken cancellationToken = default)
    {
        bool isAdding = request.Id is null;

        // Получаем всех пациентов и ищем нужного
        var allPatients = await patientsRepository.GetAllAsync(cancellationToken);
        Patient? patient = null;

        if (isAdding)
        {
            patient = allPatients.FirstOrDefault(p =>
                p.PhoneNumber == request.PhoneNumber && p.Name == request.Name);

            if (patient is not null)
                return new(409, $"Patient with phone number {request.PhoneNumber} already exists.");
        }
        else
        {
            patient = allPatients.FirstOrDefault(p => p.Id.Value == request.Id!.Value);
            if (patient is null)
                return new(404, "Patient not found.");

            patient.Name = request.Name;
            patient.Birthday = request.Birthday;
            patient.Gender = request.Gender;
            patient.PhoneNumber = request.PhoneNumber;
            patient.InsuranceNumber = request.InsuranceNumber;
        }

        if (isAdding)
        {
            patient = new Patient
            {
                Name = request.Name,
                Birthday = request.Birthday,
                Gender = request.Gender,
                PhoneNumber = request.PhoneNumber,
                InsuranceNumber = request.InsuranceNumber,
                Appointments = []
            };

            await patientsRepository.AddAsync(patient, cancellationToken);
        }

        await patientsRepository.SaveChanges(cancellationToken);
        return new(200, "OK");
    }
}
