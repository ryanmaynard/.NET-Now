using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Features.Projects.Commands.CreateProject;

public record CreateProjectCommand(
    string Name,
    string? Description,
    DateTime? StartDate,
    DateTime? EndDate) : IRequest<Result<ProjectDto>>;

public record ProjectDto(
    Guid Id,
    string Name,
    string? Description,
    string Status,
    DateTime? StartDate,
    DateTime? EndDate,
    Guid OwnerId,
    DateTime CreatedAt);

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Result<ProjectDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateProjectCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<ProjectDto>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Result<ProjectDto>.Failure("User not authenticated");
        }

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = ProjectStatus.Active,
            OwnerId = _currentUserService.UserId.Value,
            CreatedAt = _dateTimeProvider.UtcNow,
            CreatedBy = _currentUserService.Email
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new ProjectDto(
            project.Id,
            project.Name,
            project.Description,
            project.Status.ToString(),
            project.StartDate,
            project.EndDate,
            project.OwnerId,
            project.CreatedAt
        );

        return Result<ProjectDto>.Success(dto);
    }
}
