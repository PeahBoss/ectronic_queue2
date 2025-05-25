

namespace ElectronicQueue.Application.Doctors
{
    public class DoctorConverter : IConverter<Doctor, DoctorResponse>
    {
        public DoctorResponse ToResponse(Doctor doctor)
        {
            return new DoctorResponse
            {
                Id = doctor.Id.Value,
                Name = doctor.Name,
                PhoneNumber = doctor.PhoneNumber,
                OfficeNumber = doctor.OfficeNumber,
                WorkSchedule = doctor.WorkSchedule,
                Specializations = doctor.Specializations
                    .Select(s => s.Name)
                    .ToList()
            };
        }
    }
}
