[![](https://img.shields.io/nuget/v/soenneker.blazor.utils.interopeventlistener.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.blazor.utils.interopeventlistener/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.utils.interopeventlistener/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.utils.interopeventlistener/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.blazor.utils.interopeventlistener.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.blazor.utils.interopeventlistener/)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Blazor.Utils.InteropEventListener
### Manages the registration, removal, and disposal of .NET object references used for interop event listeners.

Handles warnings for potential duplicate registrations and providing methods for cleanup. The class is equipped with asynchronous and synchronous disposal mechanisms, as well as methods for adding event listeners with generic callback functions.

## Installation

```
dotnet add package Soenneker.Blazor.Utils.InteropEventListener
```
