using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Projects.Commands.CreateProject;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Projects.Commands.UpdateProject;

public record UpdateProjectCommand(
    Guid Id,
    string Name,
    string? Description,
    ProjectStatus Status,
    DateTime? StartDate,
    DateTime? EndDate) : IRequest<Result<ProjectDto>>;

public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, Result<ProjectDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateProjectCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<ProjectDto>> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Result<ProjectDto>.Failure("User not authenticated");
        }

        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (project == null)
        {
            return Result<ProjectDto>.Failure("Project not found");
        }

        if (project.OwnerId != _currentUserService.UserId.Value &&
            !_currentUserService.IsInRole("Admin"))
        {
            return Result<ProjectDto>.Failure("You don't have permission to update this project");
        }

        project.Name = request.Name;
        project.Description = request.Description;
        project.Status = request.Status;
        project.StartDate = request.StartDate;
        project.EndDate = request.EndDate;
        project.UpdatedAt = _dateTimeProvider.UtcNow;
        project.UpdatedBy = _currentUserService.Email;

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
