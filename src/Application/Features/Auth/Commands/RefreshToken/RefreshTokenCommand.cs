using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<RefreshTokenResponse>>;

public record RefreshTokenResponse(string AccessToken, string RefreshToken);

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        ITokenService tokenService,
        IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _tokenService = tokenService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var refreshToken = await _tokenService.ValidateRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (refreshToken == null)
        {
            return Result<RefreshTokenResponse>.Failure("Invalid or expired refresh token");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == refreshToken.UserId, cancellationToken);

        if (user == null || !user.IsActive)
        {
            return Result<RefreshTokenResponse>.Failure("User not found or inactive");
        }

        // Revoke old refresh token
        await _tokenService.RevokeRefreshTokenAsync(refreshToken, cancellationToken);

        // Generate new tokens
        var newRefreshTokenValue = _tokenService.GenerateRefreshToken();
        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = newRefreshTokenValue,
            UserId = user.Id,
            ExpiresAt = _dateTimeProvider.UtcNow.AddDays(7),
            CreatedAt = _dateTimeProvider.UtcNow
        };

        refreshToken.ReplacedByToken = newRefreshTokenValue;

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(user);

        return Result<RefreshTokenResponse>.Success(new RefreshTokenResponse(
            accessToken,
            newRefreshTokenValue
        ));
    }
}
