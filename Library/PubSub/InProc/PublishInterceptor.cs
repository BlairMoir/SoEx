using System.Threading.Channels;
using Castle.DynamicProxy;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SoEx.Context;

namespace SoEx.PubSub.InProc
{
    public class PublishInterceptor<I> : AsyncInterceptorBase, IPublishInterceptor<I>  where I : class
    {
        private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All, ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }  };

        private readonly ChannelWriter<PubSubEventMessage> _writer;

        public PublishInterceptor(PubSubChannel channel)
        {
            _writer = channel.Writer;            
        }

        public void Intercept(IInvocation invocation)
        {
            this.ToInterceptor().Intercept(invocation);
        }
     
        protected override async Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task> proceed)
        {

            IAmbientContext publisherContext = Container.Resolve<IAmbientContext>();
            var messageContext = new PubSubRequestContext();
            IContextFlowPolicy[] policies = Container.Resolve<IContextFlowPolicy[]>();
            foreach(IContextFlowPolicy policy in policies)
            {
                policy.Copy(publisherContext,messageContext);
            }
            
            var message = new PubSubEventMessage(){ 
                Arguments = JsonConvert.SerializeObject(invocation.Arguments,jsonSerializerSettings),
                TargetType = typeof(I).AssemblyQualifiedName,
                Method = invocation.Method.Name,
                Context = messageContext.SerilalizeContext()           
            };

            await _writer.WriteAsync(message);            
        }
        protected override Task<TResult> InterceptAsync<TResult>(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed) => throw new NotImplementedException();
    }
}
