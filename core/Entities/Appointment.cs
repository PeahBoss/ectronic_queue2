namespace ElectronicQueue.Core.Entities;

public class Appointment : IEntity
{
    public Id Id { get; set; } = null!;

    public DateTime? AppointmentDate { get; set; } = null;
    public string? ClinicalRecords { get; set; }
    
    public virtual Doctor Doctor { get; set; } = null!;
    public virtual Patient Patient { get; set; } = null!;
}