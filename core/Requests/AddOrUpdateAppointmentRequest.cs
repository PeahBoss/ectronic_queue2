namespace ElectronicQueue.Core.Requests;

public record AddOrUpdateAppointmentRequest(
    Guid? Id,
    DateTime? AppointmentDate,
    string? ClinicalRecords,
    Guid DoctorId,
    Guid PatientId
) : IRequest;