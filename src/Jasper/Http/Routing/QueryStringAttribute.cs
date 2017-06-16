using System;

namespace JasperHttp.Routing
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class QueryStringAttribute : Attribute
    {
    }
}
