using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Project : BaseAuditableEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Active;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public required Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;
}
