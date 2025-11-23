using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<Result<LoginResponse>>;

public record LoginResponse(Guid UserId, string Email, string AccessToken, string RefreshToken);

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public LoginCommandHandler(
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

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Result<LoginResponse>.Failure("Invalid email or password");
        }

        if (!user.IsActive)
        {
            return Result<LoginResponse>.Failure("User account is inactive");
        }

        user.LastLoginAt = _dateTimeProvider.UtcNow;

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

        return Result<LoginResponse>.Success(new LoginResponse(
            user.Id,
            user.Email,
            accessToken,
            refreshTokenValue
        ));
    }
}
