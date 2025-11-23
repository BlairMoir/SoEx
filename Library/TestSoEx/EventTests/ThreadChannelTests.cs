using SoEx.Exceptions;
using SoEx.Test;

namespace SoEx.TestSoEx.EventTests;

public class ThreadChannelTestEnvironment : TestEnvironmentBase { }

[Parallelizable(ParallelScope.All)]
public class ThreadChannelTests
{
    private TimeSpan _timeout = TimeSpan.FromSeconds(5);

    [Test]
    public async Task GivenReceiverCanReceiveMessages_WhenSenderSendsAnEvent_ThenReceiverProcessesTheEvent()
    {
        // arrange
        string message = Guid.NewGuid().ToString();
        EventMonitor eventMonitor = new();
        EventTestEnvironment environment = new EventTestEnvironmentBuilder()
            .WithSender<ISenderManager, SenderManager>()
            .WithReceiver<IReceiverManager, ReceiverManager>()
            .WithEventMonitor(eventMonitor)
            .WithChannelEvent<ITheEvent>()
            .Build();
        Func<ISenderManager, Task> serviceRunner = ServiceRunner.Create<ISenderManager>(async proxy =>
        {
            // act
            await proxy.SendMessage(message);

            // assert
            await eventMonitor.WaitForEventAsync(message, _timeout);
        });
        await environment.TestService(serviceRunner);
    }

    [Test]
    public async Task GivenReceiverCanReceiveMessagesOfTheSameType_WhenSenderSendsMultipleEvents_ThenReceiverProcessesAllEvents()
    {
        // arrange
        int messageCount = 5;
        IEnumerable<string> messages = Enumerable.Range(0, messageCount)
            .Select(_ => Guid.NewGuid().ToString())
            .ToArray();
        EventMonitor eventMonitor = new();
        EventTestEnvironment environment = new EventTestEnvironmentBuilder()
            .WithSender<ISenderManager, SenderManager>()
            .WithReceiver<IReceiverManager, ReceiverManager>()
            .WithEventMonitor(eventMonitor)
            .WithChannelEvent<ITheEvent>()
            .Build();
        Func<ISenderManager, Task> serviceRunner = ServiceRunner.Create<ISenderManager>(async proxy =>
        {
            // act
            foreach (string message in messages)
            {
                await proxy.SendMessage(message);
            }

            // assert
            foreach (string message in messages)
            {
                await eventMonitor.WaitForEventAsync(message, _timeout);
            }
        });
        await environment.TestService(serviceRunner);
    }

    [Test]
    public async Task GivenReceiverCanReceiveMessagesOfDifferentTypes_WhenSenderSendsMultipleEventTypes_ThenReceiverProcessesAllEvents()
    {
        // arrange
        string theMessage = Guid.NewGuid().ToString();
        string anotherMessage = Guid.NewGuid().ToString();
        EventMonitor eventMonitor = new();
        EventTestEnvironment environment = new EventTestEnvironmentBuilder()
            .WithSender<IMultiEventSenderManager, MultiEventSenderManager>()
            .WithReceiver<IMultiEventReceiverManager, MultiEventReceiverManager>()
            .WithEventMonitor(eventMonitor)
            .WithChannelEvent<ITheEvent>()
            .WithChannelEvent<IAnotherEvent>()
            .Build();
        Func<IMultiEventSenderManager, Task> serviceRunner = ServiceRunner.Create<IMultiEventSenderManager>(async proxy =>
        {
            // act
            await proxy.SendTheMessage(theMessage);
            await proxy.SendAnotherMessage(anotherMessage);

            // assert
            await eventMonitor.WaitForEventAsync(anotherMessage, _timeout);
            await eventMonitor.WaitForEventAsync(theMessage, _timeout);
        });
        await environment.TestService(serviceRunner);
    }

    [Test]
    public async Task GivenThereIsNoReceiver_WhenSenderSendsAnEvent_ThenClientExceptionIsThrown()
    {
        // arrange
        string message = Guid.NewGuid().ToString();
        EventMonitor eventMonitor = new();
        EventTestEnvironment environment = new EventTestEnvironmentBuilder()
            .WithSender<ISenderManager, SenderManager>()
            .WithReceiver<IReceiverManager, ReceiverManager>()
            .WithEventMonitor(eventMonitor)
            .Build();
        Func<ISenderManager, Task> serviceRunner = ServiceRunner.Create<ISenderManager>(async proxy =>
        {
            // act
            AsyncTestDelegate action = async () => await proxy.SendMessage(message);

            // assert
            Assert.ThrowsAsync<ClientException>(action);
            await Task.CompletedTask;
        });
        await environment.TestService(serviceRunner);
    }

    [Test]
    public async Task GivenSenderIsReceiver_WhenSenderSendsAnEvent_ThenInvalidOperationExceptionIsThrown()
    {
        // arrange
        string message = Guid.NewGuid().ToString();
        EventMonitor eventMonitor = new();
        EventTestEnvironment environment = new EventTestEnvironmentBuilder()
            .WithReceiver<ISenderReceiverManager, SenderReceiverManager>()
            .WithEventMonitor(eventMonitor)
            .WithChannelEvent<ITheEvent>()
            .Build();
        Func<ISenderReceiverManager, Task> serviceRunner = ServiceRunner.Create<ISenderReceiverManager>(async proxy =>
        {
            // act
            AsyncTestDelegate action = async () => await proxy.SendMessage(message);

            // assert
            Assert.ThrowsAsync<InvalidOperationException>(action);
            await Task.CompletedTask;
        });
        await environment.TestService(serviceRunner);
    }

    [Test]
    public async Task GivenSenderHasMultipleReceivers_WhenSenderSendsAnEvent_ThenTheMessageIsOnlyProcessedOnce()
    {
        // arrange
        string message = Guid.NewGuid().ToString();
        EventMonitor eventMonitor = new();
        EventTestEnvironment environment = new EventTestEnvironmentBuilder()
            .WithSender<ISenderManager, SenderManager>()
            .WithReceiver<IReceiverManager, ReceiverManager>()
            .WithReceiver<IReceiverManager2, ReceiverManager2>()
            .WithEventMonitor(eventMonitor)
            .WithChannelEvent<ITheEvent>()
            .Build();
        Func<ISenderManager, Task> serviceRunner = ServiceRunner.Create<ISenderManager>(async proxy =>
        {
            // act
            await proxy.SendMessage(message);

            // assert
            int successCount = await TaskVerifier.CountSuccessfulTasks(
                eventMonitor.WaitForEventAsync(message, _timeout),
                eventMonitor.WaitForEventAsync($"{message}-2", _timeout));
            Assert.That(successCount, Is.EqualTo(1));
        });
        await environment.TestService(serviceRunner);
    }

    [Test]
    public async Task GivenReceiverHasMultipleSenders_WhenSendersSendEvents_ThenReceiverProcessesAllEvents()
    {
        // arrange
        string theMessage = Guid.NewGuid().ToString();
        string anotherMessage = Guid.NewGuid().ToString();
        EventMonitor eventMonitor = new();
        EventTestEnvironment environment = new EventTestEnvironmentBuilder()
            .WithSender<ISenderManager, SenderManager>()
            .WithSender<IAnotherSenderManager, AnotherSenderManager>()
            .WithSenderParent<ISenderParentManager, SenderParentManager>()
            .WithReceiver<IMultiEventReceiverManager, MultiEventReceiverManager>()
            .WithEventMonitor(eventMonitor)
            .WithChannelEvent<ITheEvent>()
            .WithChannelEvent<IAnotherEvent>()
            .Build();
        Func<ISenderParentManager, Task> serviceRunner = ServiceRunner.Create<ISenderParentManager>(async proxy =>
        {
            // act
            await proxy.SendTheMessage(theMessage);
            await proxy.SendAnotherMessage(anotherMessage);

            // assert
            await eventMonitor.WaitForEventAsync(theMessage, _timeout);
            await eventMonitor.WaitForEventAsync(anotherMessage, _timeout);
        });
        await environment.TestService(serviceRunner);
    }

    [Test]
    public async Task GivenContractContainsMultipleEvents_WhenSenderSendsAllEvents_TheyAreProcessedByAppropriateReceiver()
    {
        // arrange
        string theMessage = Guid.NewGuid().ToString();
        string anotherMessage = Guid.NewGuid().ToString();
        string separateMessage = Guid.NewGuid().ToString();
        EventMonitor eventMonitor = new();
        EventTestEnvironment environment = new EventTestEnvironmentBuilder()
            .WithSender<IComboSenderManager, ComboSenderManager>()
            .WithReceiver<IComboReceiverManager, ComboReceiverManager>()
            .WithEventMonitor(eventMonitor)
            .WithChannelEvent<IComboEvent>()
            .WithChannelEvent<ISeparateEvent>()
            .Build();
        Func<IComboSenderManager, Task> serviceRunner = ServiceRunner.Create<IComboSenderManager>(async proxy =>
        {
            // act
            await proxy.SendTheMessage(theMessage);
            await proxy.SendAnotherMessage(anotherMessage);
            await proxy.SendSeparateMessage(separateMessage);

            // assert
            await eventMonitor.WaitForEventAsync($"{nameof(IComboEvent.OnTheEvent)}-{theMessage}", _timeout);
            await eventMonitor.WaitForEventAsync($"{nameof(IComboEvent.OnAnotherEvent)}-{anotherMessage}", _timeout);
            await eventMonitor.WaitForEventAsync($"{nameof(ISeparateEvent.OnSeparateEvent)}-{separateMessage}", _timeout);
        });
        await environment.TestService(serviceRunner);
    }
}
