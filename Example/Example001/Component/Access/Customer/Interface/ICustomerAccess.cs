using System.ServiceModel;
using System.Threading.Tasks;

namespace Example001.Access.Customer.Interface
{
    [ServiceContract]
    public interface ICustomerAccess
    {
        [OperationContract]
        Task Filter();
    }
}
