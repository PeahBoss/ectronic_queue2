namespace ectronic_queue.Core.Entities;

public class Doctor : IEntity
{
    public Id Id { get; set; } = null!;


    public string Name { get; set; } = null!;
    public string Specialization { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string OfficeNumber { get; set; } = null!;
    public string WorkSchedule { get; set; } = null!;

} 