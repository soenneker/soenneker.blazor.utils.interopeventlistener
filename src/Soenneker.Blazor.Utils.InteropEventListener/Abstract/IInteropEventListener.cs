using System;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Blazor.Utils.EventListeningInterop.Abstract;

namespace Soenneker.Blazor.Utils.InteropEventListener.Abstract;

/// <summary>
/// Manages the registration, removal, and disposal of .NET object references used for interop event listeners. Handles warnings for potential duplicate registrations and providing methods for cleanup. The class is equipped with asynchronous and synchronous disposal mechanisms, as well as methods for adding event listeners with generic callback functions.
/// </summary>
public interface IInteropEventListener : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Initializes a component with the specified interop implementation.
    /// </summary>
    /// <param name="eventListeningInterop">The interop implementation used for communication with JavaScript.</param>
    void Initialize(IEventListeningInterop eventListeningInterop);

    /// <summary>
    /// Adds an event listener to a specified HTML element.
    /// </summary>
    /// <typeparam name="T">The type of the event arguments.</typeparam>
    /// <param name="functionName"></param>
    /// <param name="elementId">The ID of the HTML element to which the event listener is added.</param>
    /// <param name="eventName">The name of the event to listen for.</param>
    /// <param name="callback">The callback function to execute when the event occurs.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the event listener registration.</param>
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
    /// <param name="eventName">The name of the event to listen for.</param>
    /// <param name="callback">The callback function to be invoked when the event occurs.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the event listener registration.</param>
    /// <remarks>
    /// If the event listener is already added for the specified element and event, this method returns a completed task without re-registering.
    /// </remarks>
    ValueTask Add<TInput, TOutput>(string functionName, string elementId, string eventName, Func<TInput, ValueTask<TOutput>> callback, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an event listener from a specified HTML element by name.
    /// </summary>
    /// <param name="elementId">The ID of the HTML element from which the event listener is removed.</param>
    /// <param name="eventName">The name of the event for which the listener should be removed.</param>
    void Remove(string elementId, string eventName);

    /// <summary>
    /// Should be called whenever the component that has registered events is disposed
    /// </summary>
    /// <param name="elementId"></param>
    void DisposeForElement(string elementId);
}
