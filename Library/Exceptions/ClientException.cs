using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoEx.Exceptions
{
    public class ClientException : Exception
    {
        public ClientException(string message) : base(message) { }

        public ClientException(string message, Exception inner) : base(message, inner) { }
    }
}
