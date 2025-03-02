using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Example003.Access.Customer.Interface;
using Example003.iFx.Proxy;
using Example003.Manager.Membership.Interface;

namespace Example003.Manager.Membership.Service
{
    public class MembershipManager : IMembershipManager, IMembershipEvents
    {
        public async Task Profile()
        {
            var proxy = Proxy.ForComponent<ICustomerAccess>(this);
            await proxy.Filter();
            var publish = Proxy.ForEvent<IMembershipEvents>(this);
            await publish.OnRegistered();
        }

        public Task OnRegistered() {
            return Task.CompletedTask;
        }
    }
}
