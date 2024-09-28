using System.Diagnostics;
using Example002.Access.Customer.Interface;
using Example002.Access.Customer.Service;
using Example002.Common.Contract;
using Example002.Common.Policy;
using Example002.Manager.Membership.Interface;
using Example002.Manager.Membership.Service;
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
            Debug.Assert(Example002.iFx.Service.Context<CountContext>.Data.HopCount == 2);
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
