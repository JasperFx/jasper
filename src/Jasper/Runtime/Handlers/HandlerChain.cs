using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Baseline;
using Baseline.Reflection;
using Jasper.Attributes;
using Jasper.Configuration;
using Jasper.ErrorHandling;
using Lamar;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using GenericEnumerableExtensions = Baseline.GenericEnumerableExtensions;

namespace Jasper.Runtime.Handlers
{
    public class HandlerChain : Chain<HandlerChain, ModifyHandlerChainAttribute>, IHasRetryPolicies
    {
        public const string NotCascading = "NotCascading";

        public readonly List<MethodCall> Handlers = new List<MethodCall>();
        private GeneratedType _generatedType;

        private bool hasConfiguredFrames;

        public HandlerChain(Type messageType)
        {
            MessageType = messageType ?? throw new ArgumentNullException(nameof(messageType));

            TypeName = messageType.FullName.Replace(".", "_").Replace("+", "_");

            Description = "Message Handler for " + MessageType.FullNameInCode();
        }



        private HandlerChain(MethodCall call) : this(call.Method.MessageType())
        {
            Handlers.Add(call);
        }

        public HandlerChain(IGrouping<Type, HandlerCall> grouping) : this(grouping.Key)
        {
            Handlers.AddRange(grouping);
        }

        /// <summary>
        /// A textual description of this HandlerChain
        /// </summary>
        public override string Description { get; }

        public Type MessageType { get; }

        /// <summary>
        /// Jasper's string identification for this message type
        /// </summary>
        public string TypeName { get; }

        internal MessageHandler Handler { get; set; }

        /// <summary>
        /// After the MessageHandler for this chain is generated & compiled, this property will
        /// hold the generated source code
        /// </summary>
        public string SourceCode => _generatedType?.SourceCode;

        /// <summary>
        /// Configure the retry policies and error handling for this chain
        /// </summary>
        public RetryPolicyCollection Retries { get; set; } = new RetryPolicyCollection();

        public static HandlerChain For<T>(Expression<Action<T>> expression)
        {
            var method = ReflectionHelper.GetMethod(expression);
            var call = new MethodCall(typeof(T), method);

            return new HandlerChain(call);
        }

        public static HandlerChain For<T>(string methodName)
        {
            var handlerType = typeof(T);
            var method = handlerType.GetMethod(methodName,
                BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

            if (method == null)
                throw new ArgumentOutOfRangeException(nameof(methodName),
                    $"Cannot find method named '{methodName}' in type {handlerType.FullName}");

            var call = new MethodCall(handlerType, method);
            call.CommentText = "Core message handling method";

            return new HandlerChain(call);
        }

        internal void AssembleType(GenerationRules rules, GeneratedAssembly generatedAssembly, IContainer container)
        {
            _generatedType = generatedAssembly.AddType(TypeName, typeof(MessageHandler));
            var handleMethod = _generatedType.MethodFor(nameof(MessageHandler.Handle));
            handleMethod.Sources.Add(new MessageHandlerVariableSource(MessageType));
            handleMethod.Frames.AddRange(DetermineFrames(rules, container));

            handleMethod.DerivedVariables.Add(new Variable(typeof(Envelope),
                $"context.{nameof(IExecutionContext.Envelope)}"));

        }

        internal MessageHandler CreateHandler(IContainer container)
        {
            var handler = container.QuickBuild(_generatedType.CompiledType).As<MessageHandler>();
            handler.Chain = this;
            Handler = handler;

            return handler;
        }

        /// <summary>
        /// Used internally to create the initial list of ordered Frames
        /// that will be used to generate the MessageHandler
        /// </summary>
        /// <param name="rules"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public List<Frame> DetermineFrames(GenerationRules rules, IContainer container)
        {
            if (!Handlers.Any())
                throw new InvalidOperationException("No method handlers configured for message type " +
                                                    MessageType.FullName);

            if (!hasConfiguredFrames)
            {
                hasConfiguredFrames = true;

                applyAttributesAndConfigureMethods(rules, container);

                foreach (var attribute in MessageType.GetTypeInfo()
                    .GetCustomAttributes(typeof(ModifyHandlerChainAttribute))
                    .OfType<ModifyHandlerChainAttribute>()) attribute.Modify(this, rules);

                foreach (var attribute in MessageType.GetTypeInfo().GetCustomAttributes(typeof(ModifyChainAttribute))
                    .OfType<ModifyChainAttribute>()) attribute.Modify(this, rules, container);
            }

            var cascadingHandlers = determineCascadingMessages().ToArray();

            // The Enqueue cascading needs to happen before the post processors because of the
            // transactional & outbox support
            return Middleware.Concat(Handlers).Concat(cascadingHandlers).Concat(Postprocessors).ToList();
        }

        private IEnumerable<CaptureCascadingMessages> determineCascadingMessages()
        {
            var i = 0;

            foreach (var handler in Handlers)
            foreach (var create in handler.Creates)
            {
                if (create.IsNotCascadingMessage()) continue;

                // FUGLY. Jeremy is very ashamed of this code
                if (create.VariableType == typeof(object) || create.VariableType == typeof(object[]) ||
                    create.VariableType == typeof(IEnumerable<object>))
                    create.OverrideName("outgoing" + ++i);


                yield return new CaptureCascadingMessages(create);
            }
        }

        public override MethodCall[] HandlerCalls()
        {
            return Handlers.ToArray();
        }

        /// <summary>
        /// Add a secondary message handler for the message type that will
        /// execute after the primary action(s)
        /// </summary>
        /// <param name="call"></param>
        public void AddAbstractedHandler(HandlerCall call)
        {
            var clone = call.Clone(MessageType);

            Handlers.Add(clone);
        }

        public override string ToString()
        {
            return
                $"{MessageType.NameInCode()} handled by {GenericEnumerableExtensions.Join(Handlers.Select(x => $"{x.HandlerType.NameInCode()}.{x.Method.Name}()"), ", ")}";
        }

        public override bool ShouldFlushOutgoingMessages()
        {
            return false;
        }

        public MessageHandler AttachPreBuiltHandler(GenerationRules rules, IContainer container, Type[] handlerTypes)
        {
            var fullName = $"{rules.ApplicationNamespace}.{TypeName}";
            var handlerType = handlerTypes.FirstOrDefault(x => x.FullName == fullName);

            if (handlerType == null) return null;

            Handler = (MessageHandler) container.QuickBuild(handlerType);
            Handler.Chain = this;

            return Handler;
        }
    }
}
