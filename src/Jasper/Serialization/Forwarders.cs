using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;

namespace Jasper.Serialization
{
    internal class Forwarders
    {
        public Dictionary<Type, Type> Relationships { get; } = new Dictionary<Type, Type>();

        public void Add(Type type)
        {
            var forwardedType = type
                .FindInterfaceThatCloses(typeof(IForwardsTo<>))
                .GetGenericArguments()
                .Single();

            if (Relationships.ContainsKey(type))
            {
                Relationships[type] = forwardedType;
            }
            else
            {
                Relationships.Add(type, forwardedType);
            }
        }
    }
}
