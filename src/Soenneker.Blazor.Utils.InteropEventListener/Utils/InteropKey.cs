using System;

namespace Soenneker.Blazor.Utils.InteropEventListener.Utils;

internal readonly struct InteropKey : IEquatable<InteropKey>
{
    public readonly string ElementId;
    public readonly string EventName;

    public InteropKey(string elementId, string eventName)
    {
        ElementId = elementId;
        EventName = eventName;
    }

    public bool Equals(InteropKey other) =>
        StringComparer.Ordinal.Equals(ElementId, other.ElementId) && StringComparer.Ordinal.Equals(EventName, other.EventName);

    public override bool Equals(object? obj) => obj is InteropKey other && Equals(other);

    public override int GetHashCode()
    {
        // Ordinal hashing; fast and stable.
        int h1 = StringComparer.Ordinal.GetHashCode(ElementId);
        int h2 = StringComparer.Ordinal.GetHashCode(EventName);
        return HashCode.Combine(h1, h2);
    }
}