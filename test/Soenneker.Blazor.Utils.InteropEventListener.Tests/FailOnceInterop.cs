using System;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Blazor.Utils.EventListeningInterop.Abstract;

namespace Soenneker.Blazor.Utils.InteropEventListener.Tests;

internal sealed class FailOnceInterop : IEventListeningInterop
{
    public int CallCount { get; private set; }

    public ValueTask AddEventListener(string functionName, string elementId, string eventName, object dotNetCallback,
        CancellationToken cancellationToken = default)
    {
        CallCount++;

        return CallCount == 1
            ? ValueTask.FromException(new InvalidOperationException("Registration failed."))
            : ValueTask.CompletedTask;
    }
}
