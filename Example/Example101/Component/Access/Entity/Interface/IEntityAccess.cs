using System.Threading.Tasks;

namespace Example101.Access.Entity.Interface
{
    public interface IEntityAccess
    {
        Task<FilterResponse> Filter(FilterRequest request);
    }
}