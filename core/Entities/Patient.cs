namespace ElectronicQueue.Core.Entities;


public class Patient : IEntity
{
    public Id Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public DateTime Birthday { get; set; } = DateTime.Now;
    public Gender Gender { get; set; } = Gender.Female;
    public  string PhoneNumber { get; set; } = null!;
    public string InsuranceNumber { get; set; } = null!;


    public virtual ICollection<Appointment> Appointments { get; set; } = [];

}