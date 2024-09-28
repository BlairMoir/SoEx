using System.ServiceModel;
using System.Threading.Tasks;
using Example002.iFx.Contract;

namespace Example002.Manager.Membership.Interface
{
    [ServiceContract]
    public interface IMembershipManager : IService
    {
        [OperationContract]
        Task Profile();
    }
}
