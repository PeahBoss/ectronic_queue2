

namespace ElectronicQueue.Application.Appointments;

public class AppointmentConverter : IConverter<Appointment, AppointmentResponse>
{
    public AppointmentResponse ToResponse(Appointment appointment)
    {
        return new AppointmentResponse
        {
            Id = appointment.Id.Value,
            AppointmentDate = appointment.AppointmentDate,
            ClinicalRecords = appointment.ClinicalRecords,
            PatientId = appointment.Patient.Id.Value,
            PatientName = appointment.Patient.Name
        };
    }
}
