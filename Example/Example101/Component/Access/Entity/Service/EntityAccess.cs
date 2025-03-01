using Example101.Access.Entity.Interface;
using Example101.Access.Entity.Service.Repository;
using Example101.Access.Entity.Service.Repository.Creatures;
using Example101.Common.Contract;
using Example101.iFx.Proxy;
using Example101.iFx.Service;
using Example101.Utility.Cache.Interface;
using Microsoft.Extensions.Logging;

namespace Example101.Access.Entity.Service
{
    public class EntityAccess : IEntityAccess
    {
        ILogger<EntityAccess> _logger;
        ICreatureRepository _creatureRepository;

        public EntityAccess(ILogger<EntityAccess> logger, ICreatureRepository creatureRepository)
        {
            _logger = logger;
            _creatureRepository = creatureRepository;
        }

        public async Task<FilterResponse> Filter(FilterRequest request)
        {            
            _logger.LogInformation("User is {IdentityName}", Context<AuthContext>.Data.Principal.Identity.Name);
            
            var cacheProxy = Proxy.ForComponent<ICacheUtility>(this);
            var cacheResult = await cacheProxy.Read(BuildReadRequest());
            if(cacheResult.Found)
            {
                return new FilterResponse();    
            }

            var firstEntityType = _creatureRepository.Load<Whale>();
            var secondEntityType = _creatureRepository.Load<Dragon>();

            await cacheProxy.Store(BuildStoreRequest([..firstEntityType, ..secondEntityType]));

            return new FilterResponse();
        }

        private ReadRequest BuildReadRequest()
        {
            return new ReadRequest(){ 
                Key = Context<AuthContext>.Data.Principal.Identity.Name,
                ForService = nameof(EntityAccess) 
            };
        }

        private StoreRequest BuildStoreRequest(object[] objects)
        {
            return new StoreRequest(){ 
                Key = Context<AuthContext>.Data.Principal.Identity.Name,
                ForService = nameof(EntityAccess),
                Lifetime = TimeSpan.MaxValue,
                Bytes = [] 
            };
        }
    }
}