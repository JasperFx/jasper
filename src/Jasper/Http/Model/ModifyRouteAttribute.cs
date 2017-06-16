using System;
using Jasper.Configuration;

namespace Jasper.Http.Model
{
    public abstract class ModifyRouteAttribute : Attribute, IModifyChain<RouteChain>
    {
        public abstract void Modify(RouteChain chain);
    }
}