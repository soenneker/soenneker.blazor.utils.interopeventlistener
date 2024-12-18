using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Soenneker.Blazor.Utils.InteropEventListener.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;


namespace Soenneker.Blazor.Utils.InteropEventListener.Tests;

[Collection("Collection")]
public class InteropEventListenerTests : FixturedUnitTest
{
    private readonly IInteropEventListener _util;

    public InteropEventListenerTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<IInteropEventListener>(true);
    }
}
