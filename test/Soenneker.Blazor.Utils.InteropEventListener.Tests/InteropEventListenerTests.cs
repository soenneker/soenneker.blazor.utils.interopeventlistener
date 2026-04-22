using Soenneker.Blazor.Utils.InteropEventListener.Abstract;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.Blazor.Utils.InteropEventListener.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public class InteropEventListenerTests : HostedUnitTest
{
    private readonly IInteropEventListener _util;

    public InteropEventListenerTests(Host host) : base(host)
    {
        _util = Resolve<IInteropEventListener>(true);
    }

    [Test]
    public void Default()
    {

    }
}
