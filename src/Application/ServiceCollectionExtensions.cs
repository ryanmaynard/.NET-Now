using System.Reflection;
using Application.Common.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

/// <summary>
/// Extension methods for configuring application services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds application layer services including MediatR and FluentValidation.
    /// </summary>
    /// <remarks>
    /// Uses reflection to scan the assembly for validators and MediatR handlers.
    /// This is the standard approach for MediatR and FluentValidation registration.
    /// </remarks>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register all FluentValidation validators from this assembly
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Register MediatR handlers and pipeline behaviors
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        return services;
    }
}
