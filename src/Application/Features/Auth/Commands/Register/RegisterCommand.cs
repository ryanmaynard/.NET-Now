using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands.Register;

public record RegisterCommand(string Email, string Password, string? FirstName, string? LastName)
    : IRequest<Result<RegisterResponse>>;

public record RegisterResponse(Guid UserId, string Email, string AccessToken, string RefreshToken);

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RegisterCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<RegisterResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser != null)
        {
            return Result<RegisterResponse>.Failure("User with this email already exists");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = UserRole.User,
            CreatedAt = _dateTimeProvider.UtcNow
        };

        _context.Users.Add(user);

        var refreshTokenValue = _tokenService.GenerateRefreshToken();
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshTokenValue,
            UserId = user.Id,
            ExpiresAt = _dateTimeProvider.UtcNow.AddDays(7),
            CreatedAt = _dateTimeProvider.UtcNow
        };

        _context.RefreshTokens.Add(refreshToken);

        await _context.SaveChangesAsync(cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(user);

        return Result<RegisterResponse>.Success(new RegisterResponse(
            user.Id,
            user.Email,
            accessToken,
            refreshTokenValue
        ));
    }
}
