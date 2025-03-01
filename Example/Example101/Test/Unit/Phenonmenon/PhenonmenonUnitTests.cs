using System.Security.Claims;
using Example101.Access.Entity.Interface;
using Example101.Access.Entity.Service;
using Example101.Access.Entity.Service.Repository;
using Example101.Access.Entity.Service.Repository.Creatures;
using Example101.Common.Contract;
using Example101.Common.Policy;
using Example101.Engine.Immitation.Interface;
using Example101.Engine.Immitation.Service;
using Example101.Manager.Phenonmenon.Interface;
using Example101.Manager.Phenonmenon.Service;
using Example101.Utility.Cache.Service;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SoEx.Test;
namespace Test.Unit.Phenonmenon;

public class PhenonmenonUnitTests
{
    UnitTestEnvironment harness;

    [SetUp]
    public void Setup()
    {
        harness = new UnitTestEnvironment();
        harness.SetupServices(
            typeof(PhenonmenonManager),
            typeof(ImmitationEngine),
            typeof(EntityAccess),
            typeof(CacheUtility)
        );
        harness.SetupContextPolicy(typeof(ContextFlowPolicy));        
        harness.DependencyServiceCollection( c => c.AddScoped<ICreatureRepository,CreatureRepository>() );
    }

    private ClaimsPrincipal TestClaimsPrinciple()
    {
        IEnumerable<Claim> claims = [
            new Claim (ClaimTypes.Name,"TestName")
        ];
        var identity = new ClaimsIdentity(claims, "Basic");
        var principal = new ClaimsPrincipal(identity);
        return principal;
    }

    private void SetAuthContext()
    {
        Example101.iFx.Service.Context<AuthContext>.SetContext(new AuthContext(){  Principal = TestClaimsPrinciple()});
    }

    [Test]
    public async Task ManagerTest_NoServiceMocks()
    {
        var serviceRunner = ServiceRunner.Create<IPhenonmenonManager>(async service =>
        {
            SetAuthContext();
            await service.Observe(new ObservationRequest());
        });
        await harness.TestService(serviceRunner);
    }

    [Test]
    public async Task ManagerTest_MockedEngine()
    {
        var immitationMock = new Mock<IImmitationEngine>();
        immitationMock.Setup(x => x.Mimic(It.IsAny<MimicRequest>())).Returns(() =>
        {            
            return Task.FromResult(ResponseBuilder.Response(new MimicResponse()));
        });
        var immitationMockService = immitationMock.Object;

        var serviceRunner = ServiceRunner.Create<IPhenonmenonManager>(async service =>
        {
            SetAuthContext();
            await service.Observe(new ObservationRequest());
        });
        await harness.TestService(serviceRunner,immitationMockService);
    }

    [Test]
    public async Task EngineTest_MockedAccess()
    {
        var entityMock = new Mock<IEntityAccess>();
        entityMock.Setup(x => x.Filter(It.IsAny<FilterRequest>())).Returns(() =>
        {            
            return Task.FromResult(new FilterResponse());
        });
        var entityMockService = entityMock.Object;

        var serviceRunner = ServiceRunner.Create<IImmitationEngine>(async service =>
        {
            SetAuthContext();
            await service.Mimic(new MimicRequest());
        });
        await harness.TestService(serviceRunner,entityMockService);
    }

    [Test]
    public async Task AccessTest_MockedDependency()
    {
        var respositoryMock = new Mock<ICreatureRepository>();
        respositoryMock.Setup(x => x.Load<Whale>()).Returns(() => {
            return [new Whale(){ Id = Guid.Empty, Source = "Mock"}];
        });
        respositoryMock.Setup(x => x.Load<Dragon>()).Returns(() => {
            return [new Dragon(){ Id = Guid.Empty, Source = "Mock"}];
        });

        var serviceRunner = ServiceRunner.Create<IEntityAccess>(async service =>
        {
            SetAuthContext();
            await service.Filter(new FilterRequest());
        });
        await harness.TestService(serviceRunner,respositoryMock.Object);
    }

    [Test]
    public async Task ManagerTest_NoServiceMocks_MockedEnityAccessDependency()
    {
        var respositoryMock = new Mock<ICreatureRepository>();
        respositoryMock.Setup(x => x.Load<Whale>()).Returns(() => {
            return [new Whale(){ Id = Guid.Empty, Source = "Mock"}];
        });
        respositoryMock.Setup(x => x.Load<Dragon>()).Returns(() => {
            return [new Dragon(){ Id = Guid.Empty, Source = "Mock"}];
        });

        var serviceRunner = ServiceRunner.Create<IPhenonmenonManager>(async service =>
        {
            SetAuthContext();
            await service.Observe(new ObservationRequest());
        });
        await harness.TestService(serviceRunner, respositoryMock.Object);
    }
}