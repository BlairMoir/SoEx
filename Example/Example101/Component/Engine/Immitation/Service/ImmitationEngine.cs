using Example101.Access.Entity.Interface;
using Example101.Engine.Immitation.Interface;
using Example101.iFx.Proxy;

namespace Example101.Engine.Immitation.Service
{
    public class ImmitationEngine : IImmitationEngine
    {
        public async Task<Response<MimicResponse>> Mimic(MimicRequest request)
        {
            IEntityAccess entityAccess =  Proxy.ForComponent<IEntityAccess>(this);
            FilterResponse result = await entityAccess.Filter(new FilterRequest());
            return ResponseBuilder.Response(new MimicResponse(),[ResponseBuilder.ErrorInfo(1,"Example") ]);
        }
    }
}