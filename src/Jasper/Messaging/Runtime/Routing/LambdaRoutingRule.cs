using System;

namespace Jasper.Messaging.Runtime.Routing
{
    public class LambdaRoutingRule : IRoutingRule
    {
        private readonly Func<Type, bool> _filter;
        private readonly Func<Type, bool> _expression;
        private readonly string _description;

        public LambdaRoutingRule(string description, Func<Type, bool> filter)
        {
            _filter = filter;
            _expression = filter;
            _description = description;
        }

        public bool Matches(Type type)
        {
            return _filter(type);
        }

        public override string ToString()
        {
            return "Messages matching " + _description;
        }
    }
}