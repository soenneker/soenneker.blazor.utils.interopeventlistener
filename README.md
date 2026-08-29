[![](https://img.shields.io/nuget/v/soenneker.blazor.utils.interopeventlistener.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.blazor.utils.interopeventlistener/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.utils.interopeventlistener/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.utils.interopeventlistener/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.blazor.utils.interopeventlistener.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.blazor.utils.interopeventlistener/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.utils.interopeventlistener/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.utils.interopeventlistener/actions/workflows/codeql.yml)

# Soenneker.Blazor.Utils.InteropEventListener

Manages the registration, removal, and disposal of .NET object references used for interop event listeners. Handles warnings for potential duplicate registrations and providing methods for cleanup. The class is equipped with asynchronous disposal as well as methods for adding event listeners with generic callback functions.

## Install

```bash
dotnet add package Soenneker.Blazor.Utils.InteropEventListener
```

## Quick start

```csharp
using Soenneker.Blazor.Utils.InteropEventListener.Registrars;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
var result = services.AddInteropEventListenerAsScoped();
```

Adds `IInteropEventListener` as a scoped service.

## What you get

- `IInteropEventListener` — Manages the registration, removal, and disposal of .NET object references used for interop event listeners. Handles warnings for potential duplicate registrations and providing methods for cleanup. The class is equipped with asynchronous disposal as well as methods for adding event listeners with generic callback functions.
- `InteropEventListenerRegistrar` — Manages the registration, removal, and disposal of .NET object references used for interop event listeners.

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `IInteropEventListener.Initialize(eventListeningInterop)` | Initializes a component with the specified interop implementation. | Returns no value; the requested change is complete when the method returns. |
| `IInteropEventListener.Add(functionName, elementId, eventName, callback, cancellationToken)` | Adds an event listener to a specified HTML element. | A task that completes when the add operation is complete. |
| `IInteropEventListener.Remove(elementId, eventName)` | Removes an event listener from a specified HTML element by name. | Returns no value; the requested change is complete when the method returns. |
| `IInteropEventListener.DisposeForElement(elementId)` | Should be called whenever the component that has registered events is disposed. | Returns no value; the requested change is complete when the method returns. |
| `InteropEventListenerRegistrar.AddInteropEventListenerAsScoped(services)` | Adds `IInteropEventListener` as a scoped service. | The same service collection, so additional registrations can be chained. |

## Important behavior

- `IInteropEventListener.Add(functionName, elementId, eventName, callback, cancellationToken)`: If the event listener is already added for the specified element and event, this method returns a completed task without re-registering.

## Practical notes

- Cancellation stops pending work; it does not undo work that has already completed.
- Dispose instances you own when their scope ends so held resources can be released.
