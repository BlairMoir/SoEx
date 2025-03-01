using System.Threading.Tasks;

namespace Example101.Manager.Phenonmenon.Interface
{
    public interface IPhenonmenonManager
    {
        Task<ObservationResponse> Observe(ObservationRequest request);
    }
}