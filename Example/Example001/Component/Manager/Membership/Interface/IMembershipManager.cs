using System.ServiceModel;
using System.Threading.Tasks;

namespace Example001.Manager.Membership.Interface
{
    [ServiceContract]
    public interface IMembershipManager
    {
        [OperationContract]
        Task Profile();
    }
}
