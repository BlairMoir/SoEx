using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SoEx.Exceptions;
using SoEx.Transport.SQS;
using SoEx.Test;
using Testcontainers.LocalStack;

namespace SoEx.TestSoEx.EventTests;

public class SQSChannelTests
{
    private LocalStackContainer _localStackContainer;
    private TimeSpan _timeout = TimeSpan.FromSeconds(10);
    private TracerProvider _tracerProvider;
    private const string _activityLabel = "SoEx.TestSoEx.SQS";
    private static readonly ActivitySource _activitySource = new(_activityLabel);

    private SQSConfig _config1 => new SQSConfig
    {
        QueueUrl = GetQueueUrl("queue-1"),
        ServiceURL = _localStackContainer.GetConnectionString()
    };

    private SQSConfig _config2 => new SQSConfig
    {
        QueueUrl = GetQueueUrl("queue-2"),
        ServiceURL = _localStackContainer.GetConnectionString()
    };

    private string GetQueueUrl(string queueName)
    {
        return $"http://localhost:{_localStackContainer.GetMappedPublicPort(4566)}/000000000000/{queueName}";
    }

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
        _localStackContainer = new LocalStackBuilder()
            .WithImage("localstack/localstack:latest")
            .Build();

        await _localStackContainer.StartAsync()
            .ConfigureAwait(false);

        Environment.SetEnvironmentVariable("AWS_REGION", "us-east-1");
        Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", "test");
        Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", "test");

        await WaitForLocalStackReady();

        await CreateQueue("queue-1");
        await CreateQueue("queue-2");
    }

    [TearDown]
    public async Task Teardown()
    {
        await _localStackContainer.DisposeAsync()
            .ConfigureAwait(false);

        // Clean up environment variables
        Environment.SetEnvironmentVariable("AWS_REGION", null);
        Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", null);
        Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", null);
    }

    private async Task WaitForLocalStackReady()
    {
        Amazon.SQS.AmazonSQSClient client = new Amazon.SQS.AmazonSQSClient(
            new Amazon.Runtime.BasicAWSCredentials("test", "test"),
            new Amazon.SQS.AmazonSQSConfig
            {
                ServiceURL = _localStackContainer.GetConnectionString()
            });

        int maxAttempts = 10;
        int delayMs = 500;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await client.ListQueuesAsync(new Amazon.SQS.Model.ListQueuesRequest());
                return; // Success - LocalStack is ready
            }
            catch (Exception)
            {
                if (attempt == maxAttempts)
                {
                    throw new InvalidOperationException(
                        $"LocalStack SQS service did not become ready after {maxAttempts} attempts");
                }

                await Task.Delay(delayMs);
                delayMs *= 2; // Exponential backoff
            }
        }
    }

    private async Task CreateQueue(string queueName)
    {
        Amazon.SQS.AmazonSQSClient client = new Amazon.SQS.AmazonSQSClient(
            new Amazon.Runtime.BasicAWSCredentials("test", "test"),
            new Amazon.SQS.AmazonSQSConfig
            {
                ServiceURL = _localStackContainer.GetConnectionString()
            });

        int maxAttempts = 3;
        int delayMs = 500;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await client.CreateQueueAsync(queueName);
                return; // Success
            }
            catch (Exception)
            {
                if (attempt == maxAttempts)
                {
                    throw;
                }

                await Task.Delay(delayMs);
            }
        }
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
            .WithSQSEvent<ITheEvent>(_config1)
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
            .WithSQSEvent<ITheEvent>(_config1)
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
            .WithSQSEvent<ITheEvent>(_config1)
            .WithSQSEvent<IAnotherEvent>(_config2)
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
            .WithSQSEvent<ITheEvent>(_config1)
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
            .WithSQSEvent<ITheEvent>(_config1)
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
            .WithSQSEvent<ITheEvent>(_config1)
            .WithSQSEvent<IAnotherEvent>(_config2)
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
            .WithSQSEvent<IComboEvent>(_config1)
            .WithSQSEvent<ISeparateEvent>(_config2)
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
