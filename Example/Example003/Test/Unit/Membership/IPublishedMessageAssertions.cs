using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Test.Unit.Membership
{
    public interface IPublishedMessageAssertions
    {
        public void OnPublished(string method, object[] Arguments);
    }
}