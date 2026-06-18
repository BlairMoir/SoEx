using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Autofac;
using Microsoft.Extensions.Logging;
using SoEx.Abstractions;
using SoEx.Context;
using SoEx.Topology;

namespace SoEx.Hosting.Default
{
    public class DefaultDispatcher : IDispatcher
    {
        readonly ILogger<DefaultDispatcher> _logger;
        readonly IHostAndClientLookup _subsystemlifeTimeScope;
        readonly IAmbientContext _callerAmbientContext;
        readonly IFrameworkContext _callerFrameworkContext;
        readonly IEnumerable<IContextFlowPolicy> _policies;
        readonly ITelemetryConfidentiality _telemetryConfidentiality;
        private readonly Topology.Role _role;

        public DefaultDispatcher(ILogger<DefaultDispatcher> logger, IHostAndClientLookup subsystemlifeTimeScope, IAmbientContext callerAmbientContext,IFrameworkContext callerFrameworkContext, ITelemetryConfidentiality telemetryConfidentiality, IEnumerable<IContextFlowPolicy> policies, Topology.Role role)
        {
            _subsystemlifeTimeScope = subsystemlifeTimeScope;
            _callerAmbientContext = callerAmbientContext;
            _callerFrameworkContext = callerFrameworkContext;
            _policies = policies;
            _logger = logger;
            _telemetryConfidentiality = telemetryConfidentiality;
            _role = role;
        }

        public async Task<InvocationResponse> Dispatch<I>(InvocationRequest invocationRequest) where I : class
        {
            ((AmbientContext)_callerAmbientContext).Deserialize(invocationRequest.AmbientContext);

            using (Activity? activity = SoEx.Diagnostics.ActivitySources.Host.StartActivity($"{nameof(DefaultDispatcher)} {typeof(I)} {invocationRequest.MethodName}"))
            {
                try
                {
                    InvocationResponse invocationResponse = new InvocationResponse();
                    ISubSystemHost subSystemHost = _subsystemlifeTimeScope.For<ISubSystemHost<I>>();
                    using (var requestLifetime = subSystemHost.BeginRequestLifetimeScope())
                    {
                        IncomingFrameworkContext<I>(invocationRequest, requestLifetime);
                        IAmbientContext operationAmbientContext = requestLifetime.Resolve<IAmbientContext>();
                        FlowIncoming(_callerAmbientContext, operationAmbientContext);

                        var scopeProperties = ScopeProperties(operationAmbientContext);
                        using (_logger.BeginScope(scopeProperties))
                        {
                            AddScopePropertiesToActivity(activity, scopeProperties);
                            I host = requestLifetime.ResolveNamed<I>("Endpoint");
                            var method = typeof(I).GetMethod(invocationRequest.MethodName);
                            Debug.Assert(method is not null);
                            var parameters = method.GetParameters();
                            var arguments = ArgumentTypes(parameters, invocationRequest.Arguments);
                            var result = method.Invoke(host, arguments);
                            Debug.Assert(result is not null);

                            if (invocationRequest.TResult is null)
                            {
                                await (Task)result;
                            }
                            else
                            {
                                var responseObject = await Convert((Task)result);
                                invocationResponse.Response = responseObject;
                            }
                            FlowContextToCaller(_callerAmbientContext, operationAmbientContext);
                            invocationResponse.AmbientContext = ((AmbientContext)_callerAmbientContext).Serialize();
                            return invocationResponse;
                        }
                    }
                }
                catch (Exception ex)
                {
                    activity?.SetStatus(ActivityStatusCode.Error);
                    throw;
                }
            }
        }

        private object[] ArgumentTypes(ParameterInfo[] parameters, object[] invocationRequestArguments)
        {
            for (int arg = 0; arg < invocationRequestArguments.Length; arg++)
            {
                if(invocationRequestArguments[arg] is null)
                    continue;

                var paramType = parameters[arg].ParameterType;

                if (paramType.IsInstanceOfType(invocationRequestArguments[arg]))
                    continue;

                var targetType = Nullable.GetUnderlyingType(paramType) ?? paramType;

                if (targetType.IsEnum && invocationRequestArguments[arg] is string stringArgument)
                {
                    invocationRequestArguments[arg] = Enum.Parse(targetType, stringArgument, ignoreCase: true);
                }
                else if (targetType.IsEnum)
                {
                    invocationRequestArguments[arg] = Enum.ToObject(targetType, invocationRequestArguments[arg]);
                }
                else
                {
                    invocationRequestArguments[arg] = System.Convert.ChangeType(invocationRequestArguments[arg],
                        targetType, CultureInfo.InvariantCulture);
                }
            }

            return invocationRequestArguments;
        }

        private void IncomingFrameworkContext<I>(InvocationRequest invocationRequest, ILifetimeScope requestLifetime)
            where I : class
        {
            var callerFrameworkContext = (FrameworkContext)_callerFrameworkContext;
            callerFrameworkContext.Deserialize(invocationRequest.FrameworkContext);
            InvocationContext invocationContext = new InvocationContext(typeof(I), invocationRequest.MethodName);
            FrameworkContext operationFrameworkContext = (FrameworkContext)requestLifetime.Resolve<IFrameworkContext>();
            operationFrameworkContext.SetOrReplace(invocationContext);

            if (callerFrameworkContext.Contains<EntryContext>() && _role.HostRole == HostRole.Component )
            {
                operationFrameworkContext.SetOrReplace(callerFrameworkContext.Get<EntryContext>());
            }
            if(_role.HostRole == HostRole.EntryPoint)
            {
                if (callerFrameworkContext.Contains<EntryContext>() )
                {
                    var previousEntryInvocationContext = callerFrameworkContext.Get<EntryContext>().Entry;
                    operationFrameworkContext.SetOrReplace(new PreviousEntryContext(previousEntryInvocationContext));
                }
                operationFrameworkContext.SetOrReplace(new EntryContext(invocationContext));
            }
        }

        private static async Task<object> Convert(Task task)
        {
            await task;
            var property = task.GetType().GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
            if (property == null)
                throw new InvalidOperationException("Task does not have a return value (" + task.GetType().ToString() + ")");
            return property.GetValue(task) ?? throw new InvalidOperationException("Result property is null");
        }

        private static void AddScopePropertiesToActivity(Activity? activity, Dictionary<string, object> scopeProperties)
        {
            foreach (var property in scopeProperties)
            {
                activity?.AddTag(property.Key, property.Value);
            }
        }

        private void FlowIncoming(IAmbientContext caller, IAmbientContext invoked)
        {
            foreach (IContextFlowPolicy policy in _policies)
            {
                policy.Incoming(caller, invoked);
            }
        }

        private void FlowContextToCaller(IAmbientContext caller, IAmbientContext invoked)
        {
            foreach (IContextFlowPolicy policy in _policies)
            {
                policy.Outgoing(invoked, caller);
            }
        }

        private Dictionary<string, object> ScopeProperties(IAmbientContext invokedContext)
        {
            var flattenedProperties = _policies.SelectMany(s => s.ScopeProperties(invokedContext));
            var protectedProperties = flattenedProperties.ToDictionary( kvp => kvp.Key, kvp => (object)_telemetryConfidentiality.Protect(kvp.Value));
            return protectedProperties;
        }
    }
}
