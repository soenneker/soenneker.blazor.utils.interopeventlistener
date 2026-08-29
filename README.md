[![](https://img.shields.io/nuget/v/soenneker.blazor.utils.interopeventlistener.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.blazor.utils.interopeventlistener/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.utils.interopeventlistener/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.utils.interopeventlistener/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.blazor.utils.interopeventlistener.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.blazor.utils.interopeventlistener/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.utils.interopeventlistener/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.utils.interopeventlistener/actions/workflows/codeql.yml)

# Soenneker.Blazor.Utils.InteropEventListener

Owns the `DotNetObjectReference` callbacks used by one Blazor JavaScript event-listening interop instance.

It creates callback wrappers for one-way and request/response events, suppresses duplicate registrations by element ID and event name, rolls back failed registrations, and disposes callback references during cleanup. JavaScript listener attachment and removal remain the consuming interop’s responsibility.

## Installation

```bash
dotnet add package Soenneker.Blazor.Utils.InteropEventListener
```

Register a transient manager because each instance contains listener state and binds to one interop object:

```csharp
using Soenneker.Blazor.Utils.InteropEventListener.Registrars;

builder.Services.AddInteropEventListenerAsTransient();
```

Inject `IInteropEventListener` into the component that owns the corresponding JavaScript widget or element.

## Initialize and add a callback

Call `Initialize` once with an `IEventListeningInterop` implementation, then register callbacks after the element exists:

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (!firstRender)
        return;

    EventListener.Initialize(WidgetInterop);

    await EventListener.Add<WidgetChange>(
        "widgetInterop.addEventListener",
        _elementId,
        "change",
        change => HandleChange(change));
}

private ValueTask HandleChange(WidgetChange change)
{
    // Validate browser-provided data before using it.
    return ValueTask.CompletedTask;
}
```

The supplied interop receives the function name, element ID, event name, and a managed callback reference. Its JavaScript registration function must retain and invoke that reference using the method expected by the Soenneker Blazor invoker.

An instance cannot be rebound to a different interop object. Resolve a separate transient manager for each independent component or interop owner.

## Callbacks that return a value

Use the two-type overload when JavaScript needs a result from .NET:

```csharp
await EventListener.Add<ValidationRequest, ValidationResult>(
    "widgetInterop.addValidator",
    _elementId,
    "validate",
    request => Validate(request));
```

Registration identity is the ordinal pair `(elementId, eventName)`. A second registration for the same pair is skipped and logged as a warning, even if its function name or callback differs.

If JavaScript registration throws or is cancelled, the newly created .NET reference is removed and disposed so the same pair can be retried.

## Cleanup order

`Remove` and `DisposeForElement` release .NET callback references; they cannot remove a DOM listener because this package has no JavaScript removal-function contract.

Use this order when an element or widget is torn down:

```csharp
await WidgetInterop.RemoveEventListener(_elementId, "change");
EventListener.Remove(_elementId, "change");
```

Or destroy the JavaScript widget so it can no longer invoke callbacks, then release every callback for the element:

```csharp
await WidgetInterop.Destroy(_elementId);
EventListener.DisposeForElement(_elementId);
```

Never dispose a callback reference while JavaScript can still invoke it. Doing so leaves a live browser listener pointing at an invalid .NET object.

The DI container eventually disposes transient instances it created, but component cleanup should release JavaScript listeners and callback references as soon as their owner is removed. Cancellation only affects pending registration; it does not unregister a listener that JavaScript already attached.

Treat event payloads as untrusted browser input. Validate them before using them for authorization, file access, navigation, or other privileged behavior.
