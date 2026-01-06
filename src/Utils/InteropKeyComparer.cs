using System.Collections.Generic;

namespace Soenneker.Blazor.Utils.InteropEventListener.Utils;

internal sealed class InteropKeyComparer : IEqualityComparer<InteropKey>
{
    public static readonly InteropKeyComparer Instance = new();

    public bool Equals(InteropKey x, InteropKey y) => x.Equals(y);

    public int GetHashCode(InteropKey obj) => obj.GetHashCode();
}