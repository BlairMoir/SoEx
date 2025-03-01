using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SoEx.Context;

namespace SoEx.PubSub.Dapr
{
    public class PubSubRequestContext : AmbientContext
    {
        private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All, ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }  };
        public string SerilalizeContext()
        {
            string contextJson = JsonConvert.SerializeObject(_contexts.ToDictionary<string,object>(),jsonSerializerSettings);
            return contextJson;
        }
        public void DeserilalizeContext(string jsonString)
        {
            var contextObject = JsonConvert.DeserializeObject<Dictionary<string, object> >(jsonString,jsonSerializerSettings);
            ArgumentNullException.ThrowIfNull(contextObject);
            foreach(var key in contextObject.Keys)
            {
                _contexts.TryAdd(key,contextObject[key]);
            }
        }        
    }
}