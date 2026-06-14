using NSubstitute;
using ViaPadel.Core.Application.AppEntry;
using ViaPadel.Core.Tools.OperationResult;

namespace UnitTests;

public class DispatcherTests
{
    // Sample command used only by these tests.
    private sealed record TestCommand(string Value);

    private static Result AnyResult() => new Success<None>(new None());

    [Fact]
    public async Task DispatchAsync_ResolvesHandler_AndReturnsItsResult()
    {
        // Arrange
        var command = new TestCommand("hello");
        Result expected = AnyResult();

        var handler = Substitute.For<ICommandHandler<TestCommand>>();
        handler.HandleASyncCommand(command).Returns(Task.FromResult(expected));

        var provider = Substitute.For<IServiceProvider>();
        provider.GetService(typeof(ICommandHandler<TestCommand>)).Returns(handler);

        var dispatcher = new Dispatcher(provider);

        // Act
        Result actual = await dispatcher.DispatchAsync(command);

        // Assert
        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task DispatchAsync_PassesCommandThrough_ToHandler()
    {
        // Arrange
        var command = new TestCommand("payload");

        var handler = Substitute.For<ICommandHandler<TestCommand>>();
        handler.HandleASyncCommand(Arg.Any<TestCommand>())
               .Returns(Task.FromResult(AnyResult()));

        var provider = Substitute.For<IServiceProvider>();
        provider.GetService(typeof(ICommandHandler<TestCommand>)).Returns(handler);

        var dispatcher = new Dispatcher(provider);

        // Act
        await dispatcher.DispatchAsync(command);

        // Assert: the exact command instance reached the handler exactly once.
        await handler.Received(1).HandleASyncCommand(command);
    }

    [Fact]
    public async Task DispatchAsync_RequestsHandlerForTheCommandType()
    {
        // Arrange
        var handler = Substitute.For<ICommandHandler<TestCommand>>();
        handler.HandleASyncCommand(Arg.Any<TestCommand>())
               .Returns(Task.FromResult(AnyResult()));

        var provider = Substitute.For<IServiceProvider>();
        provider.GetService(typeof(ICommandHandler<TestCommand>)).Returns(handler);

        var dispatcher = new Dispatcher(provider);

        // Act
        await dispatcher.DispatchAsync(new TestCommand("x"));

        // Assert: it asked the container for the closed handler type, not something else.
        provider.Received(1).GetService(typeof(ICommandHandler<TestCommand>));
    }

    [Fact]
    public async Task DispatchAsync_WhenHandlerNotRegistered_Throws()
    {
        // Arrange
        // A fresh substitute returns null from GetService by default => "not registered".
        var provider = Substitute.For<IServiceProvider>();
        var dispatcher = new Dispatcher(provider);

        // Act + Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => dispatcher.DispatchAsync(new TestCommand("x")));
    }

    [Fact]
    public async Task DispatchAsync_PropagatesFailingHandlerTask()
    {
        // Arrange: handler faults -> dispatcher should not swallow it.
        var expected = new InvalidOperationException("boom");

        var handler = Substitute.For<ICommandHandler<TestCommand>>();
        handler.HandleASyncCommand(Arg.Any<TestCommand>())
               .Returns(Task.FromException<Result>(expected));

        var provider = Substitute.For<IServiceProvider>();
        provider.GetService(typeof(ICommandHandler<TestCommand>)).Returns(handler);

        var dispatcher = new Dispatcher(provider);

        // Act + Assert
        var actual = await Assert.ThrowsAsync<InvalidOperationException>(
            () => dispatcher.DispatchAsync(new TestCommand("x")));
        Assert.Same(expected, actual);
    }
}