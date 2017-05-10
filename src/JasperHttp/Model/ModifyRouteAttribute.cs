using System;
using Jasper.Configuration;

namespace JasperHttp.Model
{
    public abstract class ModifyRouteAttribute : Attribute, IModifyChain<RouteChain>
    {
        public abstract void Modify(RouteChain chain);
    }
}