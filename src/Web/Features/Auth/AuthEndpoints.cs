using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.RefreshToken;
using Application.Features.Auth.Commands.Register;
using Application.Features.Auth.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Features.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authentication");

        group.MapPost("/register", Register)
            .WithName("Register")
            .WithOpenApi();

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithOpenApi();

        group.MapPost("/refresh", RefreshToken)
            .WithName("RefreshToken")
            .WithOpenApi();

        group.MapGet("/me", GetCurrentUser)
            .RequireAuthorization()
            .WithName("GetCurrentUser")
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> Register(
        [FromBody] RegisterRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(new { errors = result.Errors });
    }

    private static async Task<IResult> Login(
        [FromBody] LoginRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(new { errors = result.Errors });
    }

    private static async Task<IResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(new { errors = result.Errors });
    }

    private static async Task<IResult> GetCurrentUser(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetCurrentUserQuery();
        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(new { errors = result.Errors });
    }
}

public record RegisterRequest(string Email, string Password, string? FirstName, string? LastName);
public record LoginRequest(string Email, string Password);
public record RefreshTokenRequest(string RefreshToken);
