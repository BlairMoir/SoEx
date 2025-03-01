using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;

namespace Example101.Utility.Cache.Interface
{
    public class ReadRequest
    {
        public required string Key {get;init;}
        public required string ForService {get;init;}        
    }
}