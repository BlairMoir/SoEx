using System.ServiceModel;
using System.Threading.Tasks;

namespace Example003.Access.Customer.Interface
{
    [ServiceContract]
    public interface ICustomerAccess
    {
        [OperationContract]
        Task Filter();
    }
}
