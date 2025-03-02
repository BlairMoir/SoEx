using System.Diagnostics;
using Example003.Access.Customer.Interface;
using Example003.Access.Customer.Service;
using Example003.Common.Contract;
using Example003.Common.Policy;
using Example003.Manager.Membership.Interface;
using Example003.Manager.Membership.Service;
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
            Debug.Assert(Example003.iFx.Service.Context<CountContext>.Data.HopCount == 2);
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
