using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class User : BaseEntity
{
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
