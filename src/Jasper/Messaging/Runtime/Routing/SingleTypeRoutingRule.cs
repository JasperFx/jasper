using System;

namespace Jasper.Messaging.Runtime.Routing
{
    public class SingleTypeRoutingRule<T> : IRoutingRule
    {
        public bool Matches(Type type)
        {
            return type == typeof (T);
        }

        public override string ToString()
        {
            return typeof(T).Name;
        }


    }
}