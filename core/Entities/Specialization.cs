namespace ElectronicQueue.Core.Entities;

public class Specialization : IEntity 
{
    public Id Id { get; set; } = null!;

    public string Name { get; set; } = null!;
    
    public virtual ICollection<Doctor> Doctors { get; set; } = [];
}