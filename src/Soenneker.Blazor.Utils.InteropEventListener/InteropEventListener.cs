using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Soenneker.Blazor.Utils.BlazorInvoker;
using Soenneker.Blazor.Utils.BlazorOutputInvoker;
using Soenneker.Blazor.Utils.EventListeningInterop.Abstract;
using Soenneker.Blazor.Utils.InteropEventListener.Abstract;
using Soenneker.Blazor.Utils.InteropEventListener.Utils;
using Soenneker.Extensions.Object;
using Soenneker.Extensions.String;

namespace Soenneker.Blazor.Utils.InteropEventListener;

///<inheritdoc cref="IInteropEventListener"/>
internal sealed class InteropEventListener : IInteropEventListener
{
    // Avoid string key allocations by using a structured key.
    private readonly Dictionary<InteropKey, IDisposable> _dotNetObjectDict = new(InteropKeyComparer.Instance);

    private readonly List<InteropKey> _keysToRemove = new(8);

    private IEventListeningInterop? _interop;
    private readonly ILogger<InteropEventListener> _logger;

    public InteropEventListener(ILogger<InteropEventListener> logger)
    {
        _logger = logger;
    }

    public void Initialize(IEventListeningInterop eventListeningInterop)
    {
        _interop ??= eventListeningInterop;
    }

    public ValueTask Add<T>(string functionName, string elementId, string eventName, Func<T, ValueTask> callback, CancellationToken cancellationToken = default)
    {
        eventName.ThrowIfNullOrEmpty();
        _interop.ThrowIfNull();

        var key = new InteropKey(elementId, eventName);

        if (_dotNetObjectDict.TryGetValue(key, out _))
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning(
                    "{name} key ({elementId}, {eventName}) for interop ({interopType}) has already been added, there may be an issue with duplicate registration",
                    nameof(InteropEventListener), elementId, eventName, _interop!.GetType());
            }

            return ValueTask.CompletedTask;
        }

        // Allocations here are expected (invoker + DotNetObjectReference) per registration.
        var dotNetObject = DotNetObjectReference.Create(new BlazorInvoker<T>(callback));
        _dotNetObjectDict.Add(key, dotNetObject);

        return _interop!.AddEventListener(functionName, elementId, eventName, dotNetObject, cancellationToken);
    }

    public ValueTask Add<TInput, TOutput>(string functionName, string elementId, string eventName, Func<TInput, ValueTask<TOutput>> callback,
        CancellationToken cancellationToken = default)
    {
        eventName.ThrowIfNullOrEmpty();
        _interop.ThrowIfNull();

        var key = new InteropKey(elementId, eventName);

        if (_dotNetObjectDict.TryGetValue(key, out _))
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning(
                    "{name} key ({elementId}, {eventName}) for interop ({interopType}) has already been added, there may be an issue with duplicate registration",
                    nameof(InteropEventListener), elementId, eventName, _interop!.GetType());
            }

            return ValueTask.CompletedTask;
        }

        var dotNetObject = DotNetObjectReference.Create(new BlazorOutputInvoker<TInput, TOutput>(callback));
        _dotNetObjectDict.Add(key, dotNetObject);

        return _interop!.AddEventListener(functionName, elementId, eventName, dotNetObject, cancellationToken);
    }

    public void Remove(string elementId, string eventName)
    {
        var key = new InteropKey(elementId, eventName);

        if (_dotNetObjectDict.Remove(key, out IDisposable? value))
        {
            value.Dispose();
        }
    }

    public void DisposeForElement(string elementId)
    {
        _keysToRemove.Clear();

        foreach ((InteropKey key, IDisposable disposable) in _dotNetObjectDict)
        {
            // Much cheaper than StartsWith + interpolated prefix string.
            if (StringComparer.Ordinal.Equals(key.ElementId, elementId))
            {
                disposable.Dispose();
                _keysToRemove.Add(key);
            }
        }

        for (int i = 0; i < _keysToRemove.Count; i++)
        {
            _dotNetObjectDict.Remove(_keysToRemove[i]);
        }

        _keysToRemove.Clear();
    }

    public void Dispose()
    {
        foreach ((_, IDisposable v) in _dotNetObjectDict)
        {
            v.Dispose();
        }

        _dotNetObjectDict.Clear();
        _keysToRemove.Clear();
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}