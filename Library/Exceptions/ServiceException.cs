using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoEx.Exceptions
{
    public class ServiceException : Exception
    {
        public ServiceException(string message, Exception inner) : base(message, inner) { }
    }
}
