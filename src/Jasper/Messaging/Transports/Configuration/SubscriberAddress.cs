using System;
using System.Collections.Generic;
using System.Linq;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Routing;

namespace Jasper.Messaging.Transports.Configuration
{
    public class SubscriberAddress : ISubscriberAddress
    {
        public Uri Uri { get; private set; }

        public Uri Alias { get; private set; }

        public SubscriberAddress(Uri uri)
        {
            Uri = uri;
        }

        public IList<IEnvelopeModifier> Modifiers { get; } = new List<IEnvelopeModifier>();

        /// <summary>
        /// Add an IEnvelopeModifier that will apply to only this channel
        /// </summary>
        /// <typeparam name="TModifier"></typeparam>
        /// <returns></returns>
        public ISubscriberAddress ModifyWith<TModifier>() where TModifier : IEnvelopeModifier, new()
        {
            return ModifyWith(new TModifier());
        }

        /// <summary>
        /// Add an IEnvelopeModifier that will apply to only this channel
        /// </summary>
        /// <param name="modifier"></param>
        /// <returns></returns>
        public ISubscriberAddress ModifyWith(IEnvelopeModifier modifier)
        {
            Modifiers.Add(modifier);

            return this;
        }

        public IList<IRoutingRule> Rules = new List<IRoutingRule>();

        public bool ShouldSendMessage(Type messageType)
        {
            return Rules.Any(x => x.Matches(messageType));
        }

        public void ReadAlias(UriAliasLookup lookups)
        {
            var real = lookups.Resolve(Uri);
            if (real != Uri)
            {
                Alias = Uri;
                Uri = real;
            }
        }

        public override string ToString()
        {
            return $"SubscriberAddress: {Uri}";
        }
    }
}
