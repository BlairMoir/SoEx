using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SoEx.Context;
using Autofac;

namespace SoEx.PubSub.Dapr
{
    public static class SubscribeListener
    {
        private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All, ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }  };

        public async static Task Process(string message)
        {
            var pubsubMessage = JsonConvert.DeserializeObject<PubSubEventMessage>(message);
            
            using(ILifetimeScope scope = Container.BeginLocalLifetimeScopeFromRoot())                    
            {
                var policies = Container.Resolve<IContextFlowPolicy[]>();
                var subscriberContext = Container.Resolve<IAmbientContext>();
                var messageContext = new PubSubRequestContext();
                messageContext.DeserilalizeContext(pubsubMessage.Context);
                foreach(var policy in policies)
                {
                    policy.Copy(messageContext, subscriberContext);
                }
                var targetType = Type.GetType(pubsubMessage.TargetType);
                Type targetArrayType = Array.CreateInstance(targetType,0).GetType();                       
               
                object targets = scope.Resolve(targetArrayType);
                System.Reflection.MethodInfo? method = targetType.GetMethod(pubsubMessage.Method);
                if(targets is object[] array)
                {
                    Debug.Assert(method is not null);
                    List<Task> subscriberTasks = new List<Task>();
                    foreach(var subscriber in array){
                        object[]? arguments = JsonConvert.DeserializeObject<object[]>(pubsubMessage.Arguments, jsonSerializerSettings);
                        object? response = method.Invoke(subscriber,arguments);   
                        if(response is Task task)  
                        {
                            subscriberTasks.Add(task);                                    
                        }    
                    }
                    await Task.WhenAll(subscriberTasks);
                }                
            }
        }
    }
}