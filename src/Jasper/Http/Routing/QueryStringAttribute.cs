using System;

namespace Jasper.Http.Routing
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class QueryStringAttribute : Attribute
    {
    }
}
