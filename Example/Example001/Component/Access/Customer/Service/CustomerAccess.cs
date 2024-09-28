using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Example001.Access.Customer.Interface;

namespace Example001.Access.Customer.Service
{
    public class CustomerAccess : ICustomerAccess
    {
        public Task Filter()
        {
            return Task.CompletedTask;
        }
    }
}
