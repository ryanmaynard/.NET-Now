using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Web.Features.Auth;

namespace Web.IntegrationTests.Features.Auth;

public class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var request = new RegisterRequest(
            $"test{Guid.NewGuid()}@example.com",
            "Password123",
            "John",
            "Doe");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        result.Should().NotBeNull();
        result!.Email.Should().Be(request.Email);
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var email = $"duplicate{Guid.NewGuid()}@example.com";
        var request = new RegisterRequest(email, "Password123", "John", "Doe");

        // Act - First registration
        await _client.PostAsJsonAsync("/api/auth/register", request);

        // Act - Second registration with same email
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var email = $"login{Guid.NewGuid()}@example.com";
        var password = "Password123";
        var registerRequest = new RegisterRequest(email, password, "John", "Doe");
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest(email, password);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest("nonexistent@example.com", "WrongPassword");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCurrentUser_WhenAuthenticated_ShouldReturnUserInfo()
    {
        // Arrange
        var email = $"currentuser{Guid.NewGuid()}@example.com";
        var registerRequest = new RegisterRequest(email, "Password123", "John", "Doe");
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<RegisterResponse>();

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", registerResult!.AccessToken);

        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCurrentUser_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public record RegisterResponse(Guid UserId, string Email, string AccessToken, string RefreshToken);
public record LoginResponse(Guid UserId, string Email, string AccessToken, string RefreshToken);
