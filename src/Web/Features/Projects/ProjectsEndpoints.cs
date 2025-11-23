using Application.Features.Projects.Commands.CreateProject;
using Application.Features.Projects.Commands.DeleteProject;
using Application.Features.Projects.Commands.UpdateProject;
using Application.Features.Projects.Queries.GetProjectById;
using Application.Features.Projects.Queries.GetProjects;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Web.Features.Projects;

public static class ProjectsEndpoints
{
    public static IEndpointRouteBuilder MapProjectsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects")
            .WithTags("Projects")
            .RequireAuthorization();

        group.MapGet("/", GetProjects)
            .WithName("GetProjects")
            .WithOpenApi();

        group.MapGet("/{id:guid}", GetProjectById)
            .WithName("GetProjectById")
            .WithOpenApi();

        group.MapPost("/", CreateProject)
            .WithName("CreateProject")
            .WithOpenApi();

        group.MapPut("/{id:guid}", UpdateProject)
            .WithName("UpdateProject")
            .WithOpenApi();

        group.MapDelete("/{id:guid}", DeleteProject)
            .WithName("DeleteProject")
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> GetProjects(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetProjectsQuery(page > 0 ? page : 1, pageSize > 0 ? pageSize : 10);
        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(new { errors = result.Errors });
    }

    private static async Task<IResult> GetProjectById(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetProjectByIdQuery(id);
        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(new { errors = result.Errors });
    }

    private static async Task<IResult> CreateProject(
        [FromBody] CreateProjectRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateProjectCommand(
            request.Name,
            request.Description,
            request.StartDate,
            request.EndDate);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/projects/{result.Value!.Id}", result.Value)
            : Results.BadRequest(new { errors = result.Errors });
    }

    private static async Task<IResult> UpdateProject(
        Guid id,
        [FromBody] UpdateProjectRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProjectCommand(
            id,
            request.Name,
            request.Description,
            request.Status,
            request.StartDate,
            request.EndDate);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(new { errors = result.Errors });
    }

    private static async Task<IResult> DeleteProject(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new DeleteProjectCommand(id);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(new { errors = result.Errors });
    }
}

public record CreateProjectRequest(
    string Name,
    string? Description,
    DateTime? StartDate,
    DateTime? EndDate);

public record UpdateProjectRequest(
    string Name,
    string? Description,
    ProjectStatus Status,
    DateTime? StartDate,
    DateTime? EndDate);
