using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Example101.Utility.Cache.Interface
{
    public interface ICacheUtility
    {
        Task<ReadResponse> Read(ReadRequest request);
        Task Store(StoreRequest request);
    }
}