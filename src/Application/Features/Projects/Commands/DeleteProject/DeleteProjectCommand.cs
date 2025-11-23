using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Projects.Commands.DeleteProject;

public record DeleteProjectCommand(Guid Id) : IRequest<Result>;

public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteProjectCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Result.Failure("User not authenticated");
        }

        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (project == null)
        {
            return Result.Failure("Project not found");
        }

        if (project.OwnerId != _currentUserService.UserId.Value &&
            !_currentUserService.IsInRole("Admin"))
        {
            return Result.Failure("You don't have permission to delete this project");
        }

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
