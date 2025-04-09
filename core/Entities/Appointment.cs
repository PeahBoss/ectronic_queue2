namespace ectronic_queue.Core.Entities;

public class Appointment : IEntity 
{
    public Id Id { get; set; } = null!;

    public virtual ICollection<Patient> Patient { get; set; } = [];
    public DateTime AppointmentDate { get; set; } = DateTime.Now;
    public Id DoctorId { get; set; } = null!;
    public int ClinicalRecords { get; set; }
    public Id QueueStatusID { get; set; }= null!;

}