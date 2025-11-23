using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Projects.Commands.CreateProject;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Projects.Queries.GetProjectById;

public record GetProjectByIdQuery(Guid Id) : IRequest<Result<ProjectDto>>;

public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, Result<ProjectDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetProjectByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ProjectDto>> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Result<ProjectDto>.Failure("User not authenticated");
        }

        var project = await _context.Projects
            .Where(p => p.Id == request.Id)
            .Select(p => new ProjectDto(
                p.Id,
                p.Name,
                p.Description,
                p.Status.ToString(),
                p.StartDate,
                p.EndDate,
                p.OwnerId,
                p.CreatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (project == null)
        {
            return Result<ProjectDto>.Failure("Project not found");
        }

        if (project.OwnerId != _currentUserService.UserId.Value &&
            !_currentUserService.IsInRole("Admin"))
        {
            return Result<ProjectDto>.Failure("You don't have permission to view this project");
        }

        return Result<ProjectDto>.Success(project);
    }
}
