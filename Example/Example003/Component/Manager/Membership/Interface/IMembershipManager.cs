using System.ServiceModel;
using System.Threading.Tasks;

namespace Example003.Manager.Membership.Interface
{
    [ServiceContract]
    public interface IMembershipManager
    {
        [OperationContract]
        Task Profile();
    }
}
