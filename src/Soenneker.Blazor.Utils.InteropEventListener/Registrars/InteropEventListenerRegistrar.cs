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
    /// Adds <see cref="IInteropEventListener"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddInteropEventListenerAsScoped(this IServiceCollection services)
    {
        services.TryAddScoped<IInteropEventListener, InteropEventListener>();

        return services;
    }
}
