using System.Diagnostics;
using Autofac;
using Example003.Access.Customer.Interface;
using Example003.Access.Customer.Service;
using Example003.Common.Contract;
using Example003.Common.Policy;
using Example003.Manager.Membership.Interface;
using Example003.Manager.Membership.Service;
using Moq;
using SoEx.PubSub;
using SoEx.Test;

namespace Test.Unit.Membership;

public class Tests
{
    UnitTestEnvironment? harness = null;


    [SetUp]
    public void Setup()
    {
        harness = new UnitTestEnvironment();
        harness.SetupServices(
            typeof(MembershipManager),
            typeof(CustomerAccess)
        );
        harness.SetupContextPolicy(typeof(ContextFlowPolicy));
        harness.DependencyContainerBuilder( builder =>
            builder.RegisterGeneric(typeof(PublishInterceptor<>)).As(typeof(IPublishInterceptor<>))
        );
    }

    [Test]
    public async Task TestManagerNoMocks()
    {
        var publishMock = new Mock<IPublishedMessageAssertions>();
        publishMock.Setup( x=> x.OnPublished(It.IsAny<string>(), It.IsAny<object[]>()))
                    .Callback( (string method, object[] arguments) => 
                    {
                        Assert.That(arguments.Length == 0);
                        Assert.That(method == nameof(IMembershipEvents.OnRegistered));                                            
                    } );  

        Debug.Assert(harness is not null);
        var serviceRunner = ServiceRunner.Create<IMembershipManager>(async service =>
        {
            await service.Profile();
        });
        await harness.TestService(serviceRunner, publishMock.Object);
    }

    [Test]
    public async Task TestManagerWithMock()
    {
        var publishMock = new Mock<IPublishedMessageAssertions>();
        publishMock.Setup( x=> x.OnPublished(It.IsAny<string>(), It.IsAny<object[]>()));  
        Debug.Assert(harness is not null);

        var customerMock = new Mock<ICustomerAccess>();
        customerMock.Setup(x => x.Filter()).Returns(() =>
        {
            Debug.Assert(Example003.iFx.Service.Context<CountContext>.Data.HopCount == 2);
            return Task.CompletedTask;
        });
        var customerMockService = customerMock.Object;

        var serviceRunner = ServiceRunner.Create<IMembershipManager>(async service =>
        {
            await service.Profile();
        });
        await harness.TestService(serviceRunner, customerMockService, publishMock.Object);
    }
}
