using Example101.Access.Entity.Service.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Example101.Access.Entity.Service
{
    public static class EntityAccessExtensions
    {
        public static void AddEntityAccessDependencies(this IServiceCollection sc)
        {
            sc.AddScoped<ICreatureRepository,CreatureRepository>();
        }        
    }
}