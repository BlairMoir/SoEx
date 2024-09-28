using System.Diagnostics;
using Example001.Access.Customer.Interface;
using Example001.Access.Customer.Service;
using Example001.Common.Contract;
using Example001.Common.Policy;
using Example001.Manager.Membership.Interface;
using Example001.Manager.Membership.Service;
using Moq;
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
    }

    [Test]
    public async Task TestManagerNoMocks()
    {
        Debug.Assert(harness is not null);
        var serviceRunner = ServiceRunner.Create<IMembershipManager>(async service =>
        {
            await service.Profile();
        });
        await harness.TestService(serviceRunner);
    }

    [Test]
    public async Task TestManagerWithMock()
    {
        Debug.Assert(harness is not null);

        var customerMock = new Mock<ICustomerAccess>();
        customerMock.Setup(x => x.Filter()).Returns(() =>
        {
            Debug.Assert(Example001.iFx.Service.Context<CountContext>.Data.HopCount == 2);
            return Task.CompletedTask;
        });
        var customerMockService = customerMock.Object;

        var serviceRunner = ServiceRunner.Create<IMembershipManager>(async service =>
        {
            await service.Profile();
        });
        await harness.TestService(serviceRunner, customerMockService);
    }
}
