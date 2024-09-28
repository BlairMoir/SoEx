using System.ServiceModel;
using System.Threading.Tasks;
using Example002.iFx.Contract;

namespace Example002.Access.Customer.Interface
{
    [ServiceContract]
    public interface ICustomerAccess : IService
    {
        [OperationContract]
        Task Filter();
    }
}
