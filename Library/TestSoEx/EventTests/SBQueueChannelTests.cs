using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SoEx.Exceptions;
using SoEx.Transport.SBQueue;
using SoEx.Test;
using SoEx.Transport.SQS;
using Testcontainers.ServiceBus;

namespace SoEx.TestSoEx.EventTests;

public class EventTestEnvironment : TestEnvironmentBase
{
    public EventTestEnvironment() : base()
    {
        GenericRegistrations([typeof(SQSChannel<>),typeof(SBQueueChannel<>)]);
    }
}

public class SBQueueChannelTests
{
    private ServiceBusContainer _serviceBusContainer;
    private TimeSpan _timeout = TimeSpan.FromSeconds(5);
    private TracerProvider _tracerProvider;
    private const string _activityLabel = "SoEx.TestSoEx.SB";
    private static readonly ActivitySource _activitySource = new(_activityLabel);

    private SBConfig _config1 => new SBConfig
    {
        ConnectionString = _serviceBusContainer.GetConnectionString(),
        Queue = "queue.1",
        SBNamespace = "System",
    };

    private SBConfig _config2 => new SBConfig
    {
        ConnectionString = _serviceBusContainer.GetConnectionString(),
        Queue = "queue.2",
        SBNamespace = "System",
    };

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _tracerProvider = Sdk.CreateTracerProviderBuilder()
            .ConfigureResource(config =>
            {
                config.AddService(_activityLabel);
            })
            .AddSource(_activityLabel)
            .AddSource("SoEx.Client")
            .AddSource("SoEx.Host")
            .AddHttpClientInstrumentation()
            .AddConsoleExporter(config =>
            {
                config.Targets = ConsoleExporterOutputTargets.Console;
            })
            .AddOtlpExporter()
            .Build();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        if (_tracerProvider != null)
        {
            _tracerProvider.ForceFlush();
            _tracerProvider.Shutdown();
            _tracerProvider.Dispose();
        }
    }

    [SetUp]
    public async Task Setup()
    {
        _serviceBusContainer = new ServiceBusBuilder()
            .WithAcceptLicenseAgreement(true)
            .WithConfig("sbConfig.json")
            .Build();
        await _serviceBusContainer.StartAsync()
            .ConfigureAwait(false);
    }

    [TearDown]
    public async Task Teardown()
    {
        await _serviceBusContainer.DisposeAsync()
            .ConfigureAwait(false);
    }

    [Test]
    public async Task GivenReceiverCanReceiveMessages_WhenSenderSendsAnEvent_ThenReceiverProcessesTheEvent()
    {
        // arrange
        using Activity activity = _activitySource.StartActivity(nameof(GivenReceiverCanReceiveMessages_WhenSenderSendsAnEvent_ThenReceiverProcessesTheEvent))!;
        string message = Guid.NewGuid().ToString();
        EventMonitor eventMonitor = new();
        EventTestEnvironment environment = new EventTestEnvironmentBuilder()
            .WithSender<ISenderManager, SenderManager>()
            .WithReceiver<IReceiverManager, ReceiverManager>()
            .WithEventMonitor(eventMonitor)
            .WithServiceBusEvent<ITheEvent>(_config1)
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
        using Activity activity = _activitySource.StartActivity(nameof(GivenReceiverCanReceiveMessagesOfTheSameType_WhenSenderSendsMultipleEvents_ThenReceiverProcessesAllEvents))!;
        int messageCount = 5;
        IEnumerable<string> messages = Enumerable.Range(0, messageCount)
            .Select(_ => Guid.NewGuid().ToString())
            .ToArray();
        EventMonitor eventMonitor = new();
        EventTestEnvironment environment = new EventTestEnvironmentBuilder()
            .WithSender<ISenderManager, SenderManager>()
            .WithReceiver<IReceiverManager, ReceiverManager>()
            .WithEventMonitor(eventMonitor)
            .WithServiceBusEvent<ITheEvent>(_config1)
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
        using Activity activity = _activitySource.StartActivity(nameof(GivenReceiverCanReceiveMessagesOfDifferentTypes_WhenSenderSendsMultipleEventTypes_ThenReceiverProcessesAllEvents))!;
        string theMessage = Guid.NewGuid().ToString();
        string anotherMessage = Guid.NewGuid().ToString();
        EventMonitor eventMonitor = new();
        EventTestEnvironment environment = new EventTestEnvironmentBuilder()
            .WithSender<IMultiEventSenderManager, MultiEventSenderManager>()
            .WithReceiver<IMultiEventReceiverManager, MultiEventReceiverManager>()
            .WithEventMonitor(eventMonitor)
            .WithServiceBusEvent<ITheEvent>(_config1)
            .WithServiceBusEvent<IAnotherEvent>(_config2)
            .Build();
        Func<IMultiEventSenderManager, Task> serviceRunner = ServiceRunner.Create<IMultiEventSenderManager>(async proxy =>
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
    public async Task GivenThereIsNoReceiver_WhenSenderSendsAnEvent_ThenClientExceptionIsThrown()
    {
        // arrange
        using Activity activity = _activitySource.StartActivity(nameof(GivenThereIsNoReceiver_WhenSenderSendsAnEvent_ThenClientExceptionIsThrown))!;
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
        using Activity activity = _activitySource.StartActivity(nameof(GivenSenderIsReceiver_WhenSenderSendsAnEvent_ThenInvalidOperationExceptionIsThrown))!;
        string message = Guid.NewGuid().ToString();
        EventMonitor eventMonitor = new();
        EventTestEnvironment environment = new EventTestEnvironmentBuilder()
            .WithReceiver<ISenderReceiverManager, SenderReceiverManager>()
            .WithEventMonitor(eventMonitor)
            .WithServiceBusEvent<ITheEvent>(_config1)
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
        using Activity activity = _activitySource.StartActivity(nameof(GivenSenderHasMultipleReceivers_WhenSenderSendsAnEvent_ThenTheMessageIsOnlyProcessedOnce))!;
        string message = Guid.NewGuid().ToString();
        EventMonitor eventMonitor = new();
        EventTestEnvironment environment = new EventTestEnvironmentBuilder()
            .WithSender<ISenderManager, SenderManager>()
            .WithReceiver<IReceiverManager2, ReceiverManager2>()
            .WithReceiver<IReceiverManager, ReceiverManager>()
            .WithEventMonitor(eventMonitor)
            .WithServiceBusEvent<ITheEvent>(_config1)
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
        using Activity activity = _activitySource.StartActivity(nameof(GivenReceiverHasMultipleSenders_WhenSendersSendEvents_ThenReceiverProcessesAllEvents))!;
        string theMessage = Guid.NewGuid().ToString();
        string anotherMessage = Guid.NewGuid().ToString();
        EventMonitor eventMonitor = new();
        EventTestEnvironment environment = new EventTestEnvironmentBuilder()
            .WithSender<ISenderManager, SenderManager>()
            .WithSender<IAnotherSenderManager, AnotherSenderManager>()
            .WithSenderParent<ISenderParentManager, SenderParentManager>()
            .WithReceiver<IMultiEventReceiverManager, MultiEventReceiverManager>()
            .WithEventMonitor(eventMonitor)
            .WithServiceBusEvent<ITheEvent>(_config1)
            .WithServiceBusEvent<IAnotherEvent>(_config2)
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
    public async Task GivenContractContainsMultipleEventMessages_WhenSenderSendsAllCompatibleEvents_TheyAreProcessedByAppropriateReceiver()
    {
        // arrange
        using Activity activity = _activitySource.StartActivity(nameof(GivenContractContainsMultipleEventMessages_WhenSenderSendsAllCompatibleEvents_TheyAreProcessedByAppropriateReceiver))!;
        string theMessage = Guid.NewGuid().ToString();
        string anotherMessage = Guid.NewGuid().ToString();
        string separateMessage = Guid.NewGuid().ToString();
        EventMonitor eventMonitor = new();
        EventTestEnvironment environment = new EventTestEnvironmentBuilder()
            .WithSender<IComboSenderManager, ComboSenderManager>()
            .WithReceiver<IComboReceiverManager, ComboReceiverManager>()
            .WithEventMonitor(eventMonitor)
            .WithServiceBusEvent<IComboEvent>(_config1)
            .WithServiceBusEvent<ISeparateEvent>(_config2)
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
