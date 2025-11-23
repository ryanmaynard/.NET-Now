using Application.Common.Interfaces;
using Application.Features.Auth.Commands.Register;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Application.UnitTests.Features.Auth;

public class RegisterCommandHandlerTests
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _context = Substitute.For<IApplicationDbContext>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _tokenService = Substitute.For<ITokenService>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();

        _handler = new RegisterCommandHandler(
            _context,
            _passwordHasher,
            _tokenService,
            _dateTimeProvider);
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterCommand("test@example.com", "Password123", null, null);
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            CreatedAt = DateTime.UtcNow
        };

        var users = new List<User> { existingUser }.AsQueryable();
        var mockSet = Substitute.For<DbSet<User>, IQueryable<User>>();
        ((IQueryable<User>)mockSet).Provider.Returns(users.Provider);
        ((IQueryable<User>)mockSet).Expression.Returns(users.Expression);
        ((IQueryable<User>)mockSet).ElementType.Returns(users.ElementType);
        ((IQueryable<User>)mockSet).GetEnumerator().Returns(users.GetEnumerator());

        _context.Users.Returns(mockSet);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("User with this email already exists");
    }

    [Fact]
    public async Task Handle_WhenValidCommand_ShouldReturnSuccessWithTokens()
    {
        // Arrange
        var command = new RegisterCommand("newuser@example.com", "Password123", "John", "Doe");
        var now = DateTime.UtcNow;

        var users = new List<User>().AsQueryable();
        var mockSet = Substitute.For<DbSet<User>, IQueryable<User>>();
        ((IQueryable<User>)mockSet).Provider.Returns(users.Provider);
        ((IQueryable<User>)mockSet).Expression.Returns(users.Expression);
        ((IQueryable<User>)mockSet).ElementType.Returns(users.ElementType);
        ((IQueryable<User>)mockSet).GetEnumerator().Returns(users.GetEnumerator());

        _context.Users.Returns(mockSet);
        _dateTimeProvider.UtcNow.Returns(now);
        _passwordHasher.HashPassword(Arg.Any<string>()).Returns("hashedpassword");
        _tokenService.GenerateRefreshToken().Returns("refresh_token_123");
        _tokenService.GenerateAccessToken(Arg.Any<User>()).Returns("access_token_123");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Email.Should().Be("newuser@example.com");
        result.Value.AccessToken.Should().Be("access_token_123");
        result.Value.RefreshToken.Should().Be("refresh_token_123");

        _context.Users.Received(1).Add(Arg.Any<User>());
        _context.RefreshTokens.Received(1).Add(Arg.Any<RefreshToken>());
        await _context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
