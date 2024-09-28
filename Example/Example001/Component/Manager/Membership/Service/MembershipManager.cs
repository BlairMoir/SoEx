using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Example001.Access.Customer.Interface;
using Example001.iFx.Proxy;
using Example001.Manager.Membership.Interface;

namespace Example001.Manager.Membership.Service
{
    public class MembershipManager : IMembershipManager
    {
        public async Task Profile()
        {
            var proxy = Proxy.ForComponent<ICustomerAccess>(this);
            await proxy.Filter();
        }
    }
}
