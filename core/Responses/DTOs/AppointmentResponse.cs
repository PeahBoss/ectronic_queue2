namespace ElectronicQueue.Core.Responses.DTOs;

public class AppointmentResponse
{
    public Guid Id { get; set; }
    public DateTime? AppointmentDate { get; set; }
    public string? ClinicalRecords { get; set; }

    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = null!;
}
