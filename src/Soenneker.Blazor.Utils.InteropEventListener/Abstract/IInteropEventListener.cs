using System;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Blazor.Utils.EventListeningInterop.Abstract;

namespace Soenneker.Blazor.Utils.InteropEventListener.Abstract;

/// <summary>
/// Owns the .NET callback references used by one JavaScript event-listening interop instance.
/// </summary>
public interface IInteropEventListener : IAsyncDisposable
{
    /// <summary>
    /// Binds this manager to the interop implementation that registers its listeners.
    /// </summary>
    /// <param name="eventListeningInterop">The interop implementation used for communication with JavaScript.</param>
    /// <remarks>An instance can be initialized repeatedly with the same object, but cannot be rebound to a different interop instance.</remarks>
    void Initialize(IEventListeningInterop eventListeningInterop);

    /// <summary>
    /// Adds an event listener to a specified HTML element.
    /// </summary>
    /// <typeparam name="T">The type of the event arguments.</typeparam>
    /// <param name="functionName">Name of the function to invoke.</param>
    /// <param name="elementId">The ID of the HTML element to which the event listener is added.</param>
    /// <param name="eventName">Name of the event to publish or subscribe to.</param>
    /// <param name="callback">The callback function to execute when the event occurs.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the event listener registration.</param>
    /// <returns>A task that completes when the add operation is complete.</returns>
    /// <remarks>
    /// If the event listener is already added for the specified element and event, this method returns a completed task without re-registering.
    /// </remarks>
    ValueTask Add<T>(string functionName, string elementId, string eventName, Func<T, ValueTask> callback, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an event listener for a specified Blazor interop event.
    /// </summary>
    /// <typeparam name="TInput">The type of the input argument for the event listener callback.</typeparam>
    /// <typeparam name="TOutput">The type of the output result for the event listener callback.</typeparam>
    /// <param name="functionName">The name of the JavaScript function to listen for.</param>
    /// <param name="elementId">The identifier of the HTML element to attach the event listener to.</param>
    /// <param name="eventName">Name of the event to publish or subscribe to.</param>
    /// <param name="callback">The callback function to be invoked when the event occurs.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the event listener registration.</param>
    /// <returns>A task that completes when the add operation is complete.</returns>
    /// <remarks>
    /// If the event listener is already added for the specified element and event, this method returns a completed task without re-registering.
    /// </remarks>
    ValueTask Add<TInput, TOutput>(string functionName, string elementId, string eventName, Func<TInput, ValueTask<TOutput>> callback, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disposes and removes the .NET callback reference for an element and event.
    /// </summary>
    /// <param name="elementId">The ID of the HTML element from which the event listener is removed.</param>
    /// <param name="eventName">The name of the event whose callback reference should be removed.</param>
    /// <remarks>The JavaScript listener must be removed before calling this method.</remarks>
    void Remove(string elementId, string eventName);

    /// <summary>
    /// Disposes all .NET callback references registered for an element.
    /// </summary>
    /// <param name="elementId">The element whose callback references should be disposed.</param>
    /// <remarks>The JavaScript listeners must be removed, or the owning widget destroyed, before calling this method.</remarks>
    void DisposeForElement(string elementId);
}
