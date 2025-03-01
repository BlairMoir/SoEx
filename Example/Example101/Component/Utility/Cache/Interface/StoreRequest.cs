using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Example101.Utility.Cache.Interface
{
    public class StoreRequest
    {
        public required string Key {get;init;}
        public required string ForService {get;init;}
        public required TimeSpan Lifetime {get;init;}
        public required byte[] Bytes {get;init;}
    }
}