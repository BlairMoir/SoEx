using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Example101.Utility.Cache.Interface;

namespace Example101.Utility.Cache.Service
{
    public class CacheUtility : ICacheUtility
    {
        public Task<ReadResponse> Read(ReadRequest request)
        {
            return Task.FromResult(new ReadResponse(){ Found = false});
        }
        public Task Store(StoreRequest request) 
        {
            return Task.CompletedTask;
        }
    }
}