using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Baseline.Reflection;
using Jasper.Attributes;
using Jasper.Configuration;
using Jasper.Persistence.Marten.Codegen;
using Jasper.Runtime.Handlers;
using Lamar;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using Marten;
using Marten.Events;
using Marten.Events.Aggregation;
using Marten.Schema;
using Oakton.Parsing;
using TypeExtensions = LamarCodeGeneration.Util.TypeExtensions;

namespace Jasper.Persistence.Marten;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class MartenEventsAttribute : ModifyChainAttribute
{
    private static readonly Type _versioningBaseType = typeof(IAggregateVersioning).Assembly.DefinedTypes.Single(x => x.Name.StartsWith("AggregateVersioning"));

    public AggregateLoadStyle LoadStyle { get; }

    public MartenEventsAttribute(AggregateLoadStyle loadStyle)
    {
        LoadStyle = loadStyle;
    }

    public MartenEventsAttribute() : this(AggregateLoadStyle.Optimistic)
    {
    }

    public Type? AggregateType { get; set; }
    public MemberInfo? AggregateIdMember { get; set; }
    public Type? CommandType { get; private set; }

    public override void Modify(IChain chain, GenerationRules rules, IContainer container)
    {
        var handlerChain = (HandlerChain)chain;
        CommandType = handlerChain.MessageType;
        AggregateType ??= DetermineAggregateType(chain);
        AggregateIdMember = DetermineAggregateIdMember(AggregateType, CommandType);
        VersionMember = DetermineVersionMember(AggregateType);

        var loader = generateLoadAggregateCode(chain);

        var firstCall = handlerChain.Handlers.First();
        firstCall.ReturnVariable?.ShouldNotBeCascaded(); // Don't automatically cascade the methods
        validateMethodSignatureForEmittedEvents(chain, firstCall, handlerChain);
        relayAggregateToHandlerMethod(loader, firstCall);
        captureEventsAndPersistSession(chain, firstCall);
    }

    private void captureEventsAndPersistSession(IChain chain, MethodCall firstCall)
    {
        // Capture and events
        if (firstCall.ReturnVariable != null)
        {
            var register = typeof(RegisterEventsFrame<>).CloseAndBuildAs<MethodCall>(firstCall.ReturnVariable, AggregateType!);
            chain.Postprocessors.Add(register);
        }

        // Check that this isn't used in combination with [Transaction]
        chain.Postprocessors.Add(MethodCall.For<IDocumentSession>(x => x.SaveChangesAsync(default)));
    }

    private void relayAggregateToHandlerMethod(MethodCall loader, MethodCall firstCall)
    {
        var aggregateVariable = new Variable(AggregateType,
            $"{loader.ReturnVariable.Usage}.{nameof(IEventStream<string>.Aggregate)}");

        if (firstCall.HandlerType == AggregateType)
        {
            // If the handle method is on the aggregate itself
            firstCall.Target = aggregateVariable;
        }
        else
        {
            firstCall.TrySetArgument(aggregateVariable);
        }
    }

    private static void validateMethodSignatureForEmittedEvents(IChain chain, MethodCall firstCall,
        HandlerChain handlerChain)
    {
        if (firstCall.Method.ReturnType == typeof(Task) || firstCall.Method.ReturnType == typeof(void))
        {
            var parameters = chain.HandlerCalls().First().Method.GetParameters();
            var stream = parameters.FirstOrDefault(x => x.ParameterType.Closes(typeof(IEventStream<>)));
            if (stream == null)
            {
                throw new InvalidOperationException(
                    $"No events are emitted from handler {handlerChain} even though it is marked as an action that would emit Marten events. Either return the events from the handler, or use the IEventStream<T> service as an argument.");
            }
        }
    }

    private MethodCall generateLoadAggregateCode(IChain chain)
    {
        chain.Middleware.Add(new EventStoreFrame());
        var loader = typeof(LoadAggregateFrame<>).CloseAndBuildAs<MethodCall>(this, AggregateType);
        chain.Middleware.Add(loader);
        return loader;
    }

    public MemberInfo? VersionMember { get; private set; }

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
        var stream = parameters.FirstOrDefault(x => x.ParameterType.Closes(typeof(IEventStream<>)));
        if (stream != null) return stream.ParameterType.GetGenericArguments().Single();

        if (parameters.Length >= 2 && parameters[1].ParameterType.IsConcrete())
        {
            return parameters[1].ParameterType;
        }

        throw new InvalidOperationException(
            $"Unable to determine a Marten aggregate type for {chain}. You may need to explicitly specify the aggregate type in a {nameof(MartenEventsAttribute)} attribute");
    }

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

}
