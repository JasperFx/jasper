using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Messaging;

namespace Jasper.Conneg
{
    public class Forwarders
    {
        public Dictionary<Type, Type> Relationships { get; } = new Dictionary<Type, Type>();

        public void Add(Type type)
        {
            var forwardedType = type
                .FindInterfaceThatCloses(typeof(IForwardsTo<>))
                .GetGenericArguments()
                .Single();

            Relationships.Add(type, forwardedType);

        }
    }
}
