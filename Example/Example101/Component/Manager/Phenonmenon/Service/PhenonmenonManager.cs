using Example101.Common.Contract;
using Example101.Engine.Immitation.Interface;
using Example101.iFx.Proxy;
using Example101.iFx.Service;
using Example101.Manager.Phenonmenon.Interface;
using Example101.Utility.Cache.Interface;


namespace Example101.Manager.Phenonmenon.Service
{
    public class PhenonmenonManager : IPhenonmenonManager
    {
        public async Task<ObservationResponse> Observe(ObservationRequest request)
        {
            var cacheProxy = Proxy.ForComponent<ICacheUtility>(this);
            var cacheResult = await cacheProxy.Read(BuildReadRequest());
            if(cacheResult.Found)
            {
                return new ObservationResponse();    
            }

            var immitationProxy = Proxy.ForComponent<IImmitationEngine>(this);
            Response<MimicResponse> response = await immitationProxy.Mimic(new MimicRequest());
            
            if(response.Result is not null)
            {
                await cacheProxy.Store(BuildStoreRequest(response.Result));
            }

            return new ObservationResponse();
        }

        private ReadRequest BuildReadRequest()
        {
            return new ReadRequest(){ 
                Key = Context<CallChainContext>.Data.CallChainId.ToString(),
                ForService = nameof(PhenonmenonManager) 
            };
        }

        private StoreRequest BuildStoreRequest(MimicResponse response)
        {
            return new StoreRequest(){ 
                Key = Context<CallChainContext>.Data.CallChainId.ToString(),
                ForService = nameof(PhenonmenonManager),
                Lifetime = TimeSpan.MaxValue,
                Bytes = [] 
            };
        }
    }
}