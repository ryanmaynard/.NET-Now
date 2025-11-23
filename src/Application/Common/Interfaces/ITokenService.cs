using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Task<RefreshToken?> ValidateRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
}
