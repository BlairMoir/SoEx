using System.Collections;
using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace SoEx.Test
{
    internal class ServiceCollectionBridge : IServiceCollection
    {
        ContainerBuilder _containerBuilder;
        public ServiceCollectionBridge(ContainerBuilder containerBuilder)
        {
            _containerBuilder = containerBuilder;
        }

        public void Add(ServiceDescriptor item)
        {
            ServiceDescriptorMapping(item);                    
        }

        private object ServiceDescriptorMapping(ServiceDescriptor item) => item switch 
        {            
            { Lifetime: ServiceLifetime.Transient, IsKeyedService: false, ImplementationType: not null } 
                =>  _containerBuilder.RegisterType(item.ImplementationType).As(item.ServiceType),
            
            { Lifetime: ServiceLifetime.Scoped, IsKeyedService: false, ImplementationType: not null } 
                =>  _containerBuilder.RegisterType(item.ImplementationType).As(item.ServiceType).InstancePerLifetimeScope(),            
            
            { Lifetime: ServiceLifetime.Singleton, IsKeyedService: false, ImplementationType: not null } 
                =>  _containerBuilder.RegisterType(item.ImplementationType).As(item.ServiceType).SingleInstance(),  
            
            _ => throw new NotImplementedException($"{nameof(ServiceCollectionBridge)} does not implement Liftime = {item.Lifetime}, Keyed = {item.IsKeyedService}")                                
        };



        public ServiceDescriptor this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int Count => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();        
        public void Clear() => throw new NotImplementedException();
        public bool Contains(ServiceDescriptor item) => throw new NotImplementedException();
        public void CopyTo(ServiceDescriptor[] array, int arrayIndex) => throw new NotImplementedException();
        public IEnumerator<ServiceDescriptor> GetEnumerator() => throw new NotImplementedException();
        public int IndexOf(ServiceDescriptor item) => throw new NotImplementedException();
        public void Insert(int index, ServiceDescriptor item) => throw new NotImplementedException();
        public bool Remove(ServiceDescriptor item) => throw new NotImplementedException();
        public void RemoveAt(int index) => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}