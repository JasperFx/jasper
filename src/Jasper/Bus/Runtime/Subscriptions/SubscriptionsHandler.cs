using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Runtime.Routing;
using Jasper.Util;

namespace Jasper.Bus.Runtime.Subscriptions
{
    // TODO -- could just fold this into IMessageRouter
    public class SubscriptionsHandler
    {
        private readonly IMessageRouter _router;

        public SubscriptionsHandler(IMessageRouter router)
        {
            _router = router;
        }

        public void Handle(SubscriptionsChanged message)
        {
            _router.ClearAll();
        }
    }


    public class SubscriptionsChanged
    {
    }
}
