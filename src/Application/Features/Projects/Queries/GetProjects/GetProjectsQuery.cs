using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Projects.Commands.CreateProject;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Projects.Queries.GetProjects;

public record GetProjectsQuery(int Page = 1, int PageSize = 10) : IRequest<Result<ProjectsListResponse>>;

public record ProjectsListResponse(List<ProjectDto> Projects, int TotalCount, int Page, int PageSize);

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, Result<ProjectsListResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetProjectsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ProjectsListResponse>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Result<ProjectsListResponse>.Failure("User not authenticated");
        }

        var query = _context.Projects.AsQueryable();

        // Non-admin users can only see their own projects
        if (!_currentUserService.IsInRole("Admin"))
        {
            query = query.Where(p => p.OwnerId == _currentUserService.UserId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var projects = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
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
            .ToListAsync(cancellationToken);

        var response = new ProjectsListResponse(projects, totalCount, request.Page, request.PageSize);

        return Result<ProjectsListResponse>.Success(response);
    }
}
