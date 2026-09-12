using Soenneker.Asyncs.Locks;
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

namespace Soenneker.Blazor.Utils.InteropEventListener;

///<inheritdoc cref="IInteropEventListener"/>
internal sealed class InteropEventListener : IInteropEventListener
{
    // Avoid string key allocations by using a structured key.
    private readonly Dictionary<InteropKey, IDisposable> _dotNetObjectDict = new(InteropKeyComparer.Instance);
    private readonly AsyncLock _sync = new();

    private IEventListeningInterop? _interop;
    private readonly ILogger<InteropEventListener> _logger;
    private bool _disposed;

    public InteropEventListener(ILogger<InteropEventListener> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Initialize(IEventListeningInterop eventListeningInterop)
    {
        ArgumentNullException.ThrowIfNull(eventListeningInterop);

        using (_sync.LockSync())
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_interop is null)
            {
                _interop = eventListeningInterop;
                return;
            }

            if (!ReferenceEquals(_interop, eventListeningInterop))
                throw new InvalidOperationException("This listener manager is already initialized with a different interop instance.");
        }
    }

    public ValueTask Add<T>(string functionName, string elementId, string eventName, Func<T, ValueTask> callback, CancellationToken cancellationToken = default)
    {
        ValidateAddArguments(functionName, elementId, eventName, callback);
        var dotNetObject = DotNetObjectReference.Create(new BlazorInvoker<T>(callback));
        return AddCore(functionName, elementId, eventName, dotNetObject, cancellationToken);
    }

    public ValueTask Add<TInput, TOutput>(string functionName, string elementId, string eventName, Func<TInput, ValueTask<TOutput>> callback,
        CancellationToken cancellationToken = default)
    {
        ValidateAddArguments(functionName, elementId, eventName, callback);
        var dotNetObject = DotNetObjectReference.Create(new BlazorOutputInvoker<TInput, TOutput>(callback));
        return AddCore(functionName, elementId, eventName, dotNetObject, cancellationToken);
    }

    public void Remove(string elementId, string eventName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(elementId);
        ArgumentException.ThrowIfNullOrWhiteSpace(eventName);

        var key = new InteropKey(elementId, eventName);
        IDisposable? value;

        using (_sync.LockSync())
            _dotNetObjectDict.Remove(key, out value);

        value?.Dispose();
    }

    public void DisposeForElement(string elementId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(elementId);
        List<IDisposable>? references = null;

        using (_sync.LockSync())
        {
            if (_disposed)
                return;


            foreach ((InteropKey key, IDisposable disposable) in _dotNetObjectDict)
            {
                if (StringComparer.Ordinal.Equals(key.ElementId, elementId))
                {
                    (references ??= []).Add(disposable);
                    _dotNetObjectDict.Remove(key);
                }
            }

        }

        if (references is not null)
        {
            foreach (IDisposable reference in references)
                reference.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        List<IDisposable> references;

        using (await _sync.Lock().ConfigureAwait(false))
        {
            if (_disposed)
                return;

            _disposed = true;
            references = [.. _dotNetObjectDict.Values];
            _dotNetObjectDict.Clear();
        }

        foreach (IDisposable reference in references)
            reference.Dispose();
    }

    private async ValueTask AddCore(string functionName, string elementId, string eventName, IDisposable dotNetObject,
        CancellationToken cancellationToken)
    {
        var key = new InteropKey(elementId, eventName);
        IEventListeningInterop interop;

        using (await _sync.Lock().ConfigureAwait(false))
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            interop = _interop ?? throw new InvalidOperationException("Initialize must be called before adding listeners.");

            if (!_dotNetObjectDict.TryAdd(key, dotNetObject))
            {
                dotNetObject.Dispose();

                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning(
                        "{name} key ({elementId}, {eventName}) for interop ({interopType}) has already been added; duplicate registration was skipped",
                        nameof(InteropEventListener), elementId, eventName, interop.GetType());
                }

                return;
            }

        }

        try
        {
            await interop.AddEventListener(functionName, elementId, eventName, dotNetObject, cancellationToken);
        }
        catch
        {
            bool removed;

            using (await _sync.Lock().ConfigureAwait(false))
            {
                removed = _dotNetObjectDict.TryGetValue(key, out IDisposable? current) && ReferenceEquals(current, dotNetObject);

                if (removed)
                    _dotNetObjectDict.Remove(key);
            }

            if (removed)
                dotNetObject.Dispose();

            throw;
        }
    }

    private static void ValidateAddArguments(string functionName, string elementId, string eventName, Delegate callback)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(functionName);
        ArgumentException.ThrowIfNullOrWhiteSpace(elementId);
        ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
        ArgumentNullException.ThrowIfNull(callback);
    }
}
