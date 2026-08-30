namespace Planforge.Domain.Entities;

public class Organization
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedOn { get; set; }

    public ICollection<Membership> Members { get; private set; } = new List<Membership>();

    private Organization() { }

    public Organization(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
        CreatedOn = DateTime.UtcNow;
    }
}