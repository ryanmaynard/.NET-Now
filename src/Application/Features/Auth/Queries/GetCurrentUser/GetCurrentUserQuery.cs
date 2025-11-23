using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Queries.GetCurrentUser;

public record GetCurrentUserQuery : IRequest<Result<UserDto>>;

public record UserDto(Guid Id, string Email, string? FirstName, string? LastName, string Role);

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<UserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetCurrentUserQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Result<UserDto>.Failure("User not authenticated");
        }

        var user = await _context.Users
            .Where(u => u.Id == _currentUserService.UserId.Value)
            .Select(u => new UserDto(u.Id, u.Email, u.FirstName, u.LastName, u.Role.ToString()))
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            return Result<UserDto>.Failure("User not found");
        }

        return Result<UserDto>.Success(user);
    }
}
