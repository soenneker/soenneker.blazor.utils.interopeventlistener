using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Blazor.Utils.InteropEventListener.Abstract;

namespace Soenneker.Blazor.Utils.InteropEventListener.Registrars;

/// <summary>
/// Manages the registration, removal, and disposal of .NET object references used for interop event listeners.
/// </summary>
public static class InteropEventListenerRegistrar
{
    /// <summary>
    /// Adds a new <see cref="IInteropEventListener"/> for each resolution.
    /// </summary>
    /// <param name="services">Service collection that receives the registration.</param>
    /// <returns>The same service collection, so additional registrations can be chained.</returns>
    public static IServiceCollection AddInteropEventListenerAsTransient(this IServiceCollection services)
    {
        services.TryAddTransient<IInteropEventListener, InteropEventListener>();

        return services;
    }

    /// <summary>
    /// Adds a new <see cref="IInteropEventListener"/> for each resolution.
    /// </summary>
    /// <remarks>This compatibility alias registers the manager as transient because its state cannot safely be shared for an entire Blazor scope.</remarks>
    /// <param name="services">Service collection that receives the registration.</param>
    /// <returns>The same service collection, so additional registrations can be chained.</returns>
    public static IServiceCollection AddInteropEventListenerAsScoped(this IServiceCollection services)
    {
        return services.AddInteropEventListenerAsTransient();
    }
}
