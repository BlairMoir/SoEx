using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Example101.Utility.Cache.Interface
{
    public class ReadResponse
    {
        public required bool Found {get; init;}
        public byte[]? Bytes {get;init;}
    }
}