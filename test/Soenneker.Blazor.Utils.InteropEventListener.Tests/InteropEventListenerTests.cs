using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using Soenneker.Blazor.Utils.EventListeningInterop.Abstract;
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
    public async ValueTask Failed_registration_can_be_retried(CancellationToken cancellationToken)
    {
        var interop = new FailOnceInterop();
        _util.Initialize(interop);

        Func<Task> firstAttempt = async () => await _util.Add<int>("events.add", "target", "change", _ => ValueTask.CompletedTask, cancellationToken: cancellationToken);

        await firstAttempt.Should().ThrowAsync<InvalidOperationException>();

        await _util.Add<int>("events.add", "target", "change", _ => ValueTask.CompletedTask, cancellationToken: cancellationToken);
        interop.CallCount.Should().Be(2);

        await _util.Add<int>("events.add", "target", "change", _ => ValueTask.CompletedTask, cancellationToken: cancellationToken);
        interop.CallCount.Should().Be(2);
        _util.Remove("target", "change");
        await _util.Add<int>("events.add", "target", "change", _ => ValueTask.CompletedTask, cancellationToken: cancellationToken);
        interop.CallCount.Should().Be(3);
    }
}
