using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public TokenService(
        IConfiguration configuration,
        IApplicationDbContext context,
        IDateTimeProvider dateTimeProvider)
    {
        _configuration = configuration;
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public string GenerateAccessToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured")));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: _dateTimeProvider.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryMinutes"] ?? "15")),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public async Task<RefreshToken?> ValidateRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

        if (refreshToken == null || !refreshToken.IsActive)
        {
            return null;
        }

        return refreshToken;
    }

    public async Task RevokeRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = _dateTimeProvider.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
