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
using Soenneker.Extensions.Object;
using Soenneker.Extensions.String;

namespace Soenneker.Blazor.Utils.InteropEventListener;

/// <inheritdoc cref="IInteropEventListener"/>
internal sealed class InteropEventListener : IInteropEventListener
{
    private readonly Dictionary<string, IDisposable> _dotNetObjectDict = [];
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

        string key = BuildKey(elementId, eventName);

        if (_dotNetObjectDict.ContainsKey(key))
        {
            Type derivedType = _interop.GetType();
            _logger.LogWarning("{name} key ({key}) for interop ({interopType}) has already been added, there may be an issue with duplicate registration", nameof(InteropEventListener), key,
                derivedType);
            return ValueTask.CompletedTask;
        }

        var dotNetObject = DotNetObjectReference.Create(new BlazorInvoker<T>(callback));

        _dotNetObjectDict.Add(key, dotNetObject);

        return _interop!.AddEventListener(functionName, elementId, eventName, dotNetObject, cancellationToken);
    }

    public ValueTask Add<TInput, TOutput>(string functionName, string elementId, string eventName, Func<TInput, ValueTask<TOutput>> callback, CancellationToken cancellationToken = default)
    {
        eventName.ThrowIfNullOrEmpty();
        _interop.ThrowIfNull();

        string key = BuildKey(elementId, eventName);

        if (_dotNetObjectDict.ContainsKey(key))
        {
            Type derivedType = _interop.GetType();
            _logger.LogWarning("{name} key ({key}) for interop ({interopType}) has already been added, there may be an issue with duplicate registration", nameof(InteropEventListener), key,
                derivedType);
            return ValueTask.CompletedTask;
        }

        var dotNetObject = DotNetObjectReference.Create(new BlazorOutputInvoker<TInput, TOutput>(callback));

        _dotNetObjectDict.Add(key, dotNetObject);

        return _interop!.AddEventListener(functionName, elementId, eventName, dotNetObject, cancellationToken);
    }

    public void Remove(string elementId, string eventName)
    {
        string key = BuildKey(elementId, eventName);

        if (_dotNetObjectDict.TryGetValue(key, out IDisposable? value))
        {
            value.Dispose();
        }

        _dotNetObjectDict.Remove(key, out _);
    }

    public void DisposeForElement(string elementId)
    {
        var objRefToBeRemoved = new List<string>();

        foreach ((string? key, IDisposable? disposable) in _dotNetObjectDict)
        {
            if (key.StartsWith($"{elementId}_"))
            {
                objRefToBeRemoved.Add(key);
                disposable.Dispose();
            }
        }

        foreach (string objRef in objRefToBeRemoved)
        {
            _dotNetObjectDict.Remove(objRef);
        }
    }

    private static string BuildKey(string elementId, string eventName)
    {
        return $"{elementId}_{eventName}";
    }

    public void Dispose()
    {
        foreach ((string _, IDisposable v) in _dotNetObjectDict)
        {
            v.Dispose();
        }

        _dotNetObjectDict.Clear();
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}