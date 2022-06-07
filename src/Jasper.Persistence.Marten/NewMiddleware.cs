using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Baseline;
using Baseline.Reflection;
using Jasper.Attributes;
using Jasper.Configuration;
using Jasper.Runtime.Handlers;
using Lamar;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using Marten;
using Marten.Events;
using Marten.Events.Aggregation;
using Marten.Schema;
using Marten.Schema.Identity;

namespace Jasper.Persistence.Marten;

public class SubmitOrder
{
    public Guid OrderId { get; set; }
    public int Version { get; set; }
}

public record OrderSubmitted(Guid CustomerId);

public class Order
{
    public Guid Id { get; set; }

    public DateTimeOffset SubmittedTime { get; set; }
}

public enum AggregateLoadStyle
{
    Optimistic,
    Exclusive
}


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class MartenEventsAttribute : ModifyChainAttribute
{
    private static Type _versioningBaseType = typeof(IAggregateVersioning).Assembly.DefinedTypes.Single(x => x.Name.StartsWith("AggregateVersioning"));

    public AggregateLoadStyle LoadStyle { get; }
    /*
     * TASKS
     * 1. Figure out the version member on the command
     * 2. Figure out the aggregate. 2nd argument?
     * 3. Figure out the aggregate id -- first cut is [Aggregate Type Name]Id, second is [AggregateId]
     */

    public MartenEventsAttribute(AggregateLoadStyle loadStyle)
    {
        LoadStyle = loadStyle;
    }

    public MartenEventsAttribute() : this(AggregateLoadStyle.Optimistic)
    {
    }

    public Type? AggregateType { get; init; }
    public string? AggregateIdMember { get; init; }

    public override void Modify(IChain chain, GenerationRules rules, IContainer container)
    {
        var handlerChain = (HandlerChain)chain;
        var aggregateType = DetermineAggregateType(chain);
        var aggregateIdMember = DetermineAggregateIdMember(aggregateType, handlerChain.MessageType);
        var versionMember = DetermineVersionMember(aggregateType);

        var loader = DetermineLoadAggregateFrame(aggregateType, aggregateIdMember, versionMember);
        chain.Middleware.Add(loader);

        var register = DetermineEventRegistrations(handlerChain);
        chain.Postprocessors.Add(register);
    }

    internal MemberInfo DetermineVersionMember(Type aggregateType)
    {
        // The first arg doesn't matter
        var versioning = _versioningBaseType.CloseAndBuildAs<IAggregateVersioning>(AggregationScope.SingleStream ,aggregateType);
        return versioning.VersionMember;
    }

    internal Type DetermineAggregateType(IChain chain)
    {
        if (AggregateType != null) return AggregateType;

        var parameters = chain.HandlerCalls().First().Method.GetParameters();
        if (parameters.Length >= 2 && parameters[1].ParameterType.IsConcrete())
        {
            return parameters[1].ParameterType;
        }

        throw new InvalidOperationException(
            $"Unable to determine a Marten aggregate type for {chain}. You may need to explicitly specify the aggregate type in a {nameof(MartenEventsAttribute)} attribute");
    }

    // 1. [Aggregate]Id convention, matches id type
    // 2. [Identity] usage, matches id type
    // 3.
    internal static MemberInfo DetermineAggregateIdMember(Type aggregateType, Type commandType)
    {
        var conventionalMemberName = $"{aggregateType.Name}Id";
        var member = commandType.GetMembers().FirstOrDefault(x => x.HasAttribute<IdentityAttribute>() || x.Name.EqualsIgnoreCase(conventionalMemberName));

        if (member == null)
        {
            throw new InvalidOperationException(
                $"Unable to determine the aggregate id for aggregate type {aggregateType.FullNameInCode()} on command type {commandType.FullNameInCode()}. Either make a property or field named '{conventionalMemberName}', or decorate a member with the {typeof(IdentityAttribute).FullNameInCode()} attribute");
        }

        return member;
    }

    // 1. With no version member
    // 2. With version number
    // 3. Using exclusive version
    internal MethodCall DetermineLoadAggregateFrame(Type aggregateType, MemberInfo aggregateIdMember,
        MemberInfo versionMember)
    {
        throw new NotImplementedException();
    }

    // 1. Sync one event
    // 2. Sync many events
    // 3. Async one event
    // 4. Async many events
    // 5. Task & takes IEventStream
    // 6. void & takes IEventStream
    // 7. void & does not take IEventStream, should blow up
    // 8. Task and does not take IEventStream, should blow up
    internal MethodCall DetermineEventRegistrations(IChain chain)
    {
        throw new NotImplementedException();
    }


}

public class OrderHandler
{
    [MartenEvents()]
    public OrderSubmitted? Handle(SubmitOrder command, Order aggregate)
    {
        throw new NotImplementedException();
    }
}

/*
 DO NOT DO ANYTHING FOR STARTING A NEW AGGREGATE
Attribute for self-applying aggregate?
Attribute for command that creates a new attribute?





*/

// TODO -- would rather have this in Marten itself
// public class StartStream<T> where T : class
// {
//     public IReadOnlyList<object> Events { get; }
//
//     public StartStream(params object[] events)
//     {
//         Events = events;
//     }
//
//     public StartStream(string? key, params object[] events)
//     {
//         Key = key;
//     }
//
//     public string? Key { get; }
//     public Guid Id { get; } = CombGuidIdGeneration.NewGuid();
//
//     public void Apply(IEventStore store)
//     {
//
//         store.StartStream<T>()
//     }
// }
