using System.Threading.Tasks;

namespace Example101.Engine.Immitation.Interface
{
    public interface IImmitationEngine
    {
        Task<Response<MimicResponse>> Mimic(MimicRequest request);
    }
}