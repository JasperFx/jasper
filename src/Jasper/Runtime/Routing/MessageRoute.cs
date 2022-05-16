using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Baseline.ImTools;
using Baseline.Reflection;
using Jasper.Attributes;
using Jasper.Configuration;
using Jasper.Runtime.Scheduled;
using Jasper.Serialization;
using Jasper.Transports.Sending;

namespace Jasper.Runtime.Routing;

internal class MessageRoute
{
    private static ImHashMap<Type, IList<IEnvelopeRule>> _rulesByMessageType = ImHashMap<Type, IList<IEnvelopeRule>>.Empty;

    public static IEnumerable<IEnvelopeRule> RulesForMessageType(Type type)
    {
        if (_rulesByMessageType.TryFind(type, out var rules))
        {
            return rules;
        }

        rules = type.GetAllAttributes<ModifyEnvelopeAttribute>().OfType<IEnvelopeRule>().ToList();
        _rulesByMessageType = _rulesByMessageType.AddOrUpdate(type, rules);

        return rules;
    }

    public IMessageSerializer Serializer { get; }
    public ISendingAgent Sender { get; }

    public IList<IEnvelopeRule> Rules { get; } = new List<IEnvelopeRule>();

    public MessageRoute(Type messageType, Endpoint endpoint) : this(endpoint.DefaultSerializer!, endpoint.Agent!, endpoint.OutgoingRules.Concat(RulesForMessageType(messageType)))
    {

    }

    public MessageRoute(IMessageSerializer serializer, ISendingAgent sender, IEnumerable<IEnvelopeRule> rules)
    {
        Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        Rules.AddRange(rules);
    }

    public Envelope CreateForSending(object message, DeliveryOptions? options, ISendingAgent localDurableQueue,
        JasperRuntime runtime)
    {
        var envelope = new Envelope(message, Sender);
        if (options != null && options.ContentType.IsNotEmpty() && options.ContentType != envelope.ContentType)
        {
            envelope.Serializer = runtime.Options.FindSerializer(options.ContentType);
        }

        foreach (var rule in Rules)
        {
            rule.Modify(envelope);
        }

        // Delivery options win
        options?.Override(envelope);

        // adjust for local, scheduled send
        if (envelope.IsScheduledForLater(DateTimeOffset.Now) && !Sender.SupportsNativeScheduledSend)
        {
            return envelope.ForScheduledSend(localDurableQueue);
        }

        return envelope;
    }
}
