using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Channels;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core.Activators.Reflection;
using Autofac.Features.ResolveAnything;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SoEx.Context;

namespace SoEx.PubSub.InProc
{
    public class SubscribeListener : BackgroundService
    {
        private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All, ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }  };

        ChannelReader<PubSubEventMessage> _reader;
        ILogger<SubscribeListener> _logger;

        public SubscribeListener(ILogger<SubscribeListener> logger, PubSubChannel channel)
        {
            _reader = channel.Reader;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(await _reader.WaitToReadAsync(stoppingToken))
            {
                try{                    
                    using(var scope = Container.BeginLocalLifetimeScopeFromRoot())                    
                    {
                        var message = await _reader.ReadAsync(stoppingToken);
                        var policies = Container.Resolve<IContextFlowPolicy[]>();
                        
                        var subscriberContext = Container.Resolve<IAmbientContext>();
                        var messageContext = new PubSubRequestContext();

                        messageContext.DeserilalizeContext(message.Context);
                        foreach(var policy in policies)
                        {
                            policy.Copy(messageContext, subscriberContext);
                        }
                        
                        var targetType = Type.GetType(message.TargetType);
                        Type targetArrayType = Array.CreateInstance(targetType,0).GetType();                       
                        object targets = scope.Resolve(targetArrayType);
                        System.Reflection.MethodInfo? method = targetType.GetMethod(message.Method);
                        if(targets is object[] array)
                        {
                            Debug.Assert(method is not null);
                            List<Task> subscriberTasks = new List<Task>();
                            foreach(var subscriber in array){
                                object[]? arguments = JsonConvert.DeserializeObject<object[]>(message.Arguments, jsonSerializerSettings);
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
                catch(Exception ex)        
                {
                    _logger.LogError(ex, "{ExceptionMessage}", ex.Message);
                }
            }            
        }        
    }
}