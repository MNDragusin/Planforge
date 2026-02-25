namespace Planforge.Domain.Entities.TaskManagement;

public class Task
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid ProjectId { get; set; }
    
    public string? Title { get; set; }
    public string? Description { get; set; }
    public Guid StatusId {get; set;}
    public Guid AssigneeUserId { get; set; }
    public Guid ReporterUserId { get; set; }
    
    public Guid PriorityId { get; set; }
    public Guid SprintId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime CompletedAt { get; set; }
    public DateTime DueDate { get; set; }
}