using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Example002.Access.Customer.Interface;
using Example002.iFx.Proxy;
using Example002.Manager.Membership.Interface;

namespace Example002.Manager.Membership.Service
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
