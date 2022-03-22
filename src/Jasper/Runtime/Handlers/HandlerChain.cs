using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
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
    public class HandlerChain : Chain<HandlerChain, ModifyHandlerChainAttribute>, IHasRetryPolicies, ICodeFile
    {
        private readonly HandlerGraph _parent;
        public const string NotCascading = "NotCascading";

        public readonly List<MethodCall> Handlers = new List<MethodCall>();

        private bool hasConfiguredFrames;
        private string _fileName;
        private Type _handlerType;
        private GeneratedType? _generatedType;

        public HandlerChain(Type messageType, HandlerGraph parent)
        {
            _parent = parent;
            MessageType = messageType ?? throw new ArgumentNullException(nameof(messageType));

            TypeName = messageType.ToSuffixedTypeName("Handler");

            Description = "Message Handler for " + MessageType.FullNameInCode();
        }



        private HandlerChain(MethodCall call, HandlerGraph parent) : this(call.Method.MessageType(), parent)
        {
            Handlers.Add(call);
        }

        public HandlerChain(IGrouping<Type, HandlerCall> grouping, HandlerGraph parent) : this(grouping.Key, parent)
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
        /// Configure the retry policies and error handling for this chain
        /// </summary>
        public RetryPolicyCollection Retries { get; set; } = new RetryPolicyCollection();

        public static HandlerChain For<T>(Expression<Action<T>> expression, HandlerGraph parent)
        {
            var method = ReflectionHelper.GetMethod(expression);
            var call = new MethodCall(typeof(T), method);

            return new HandlerChain(call, parent);
        }

        public static HandlerChain For<T>(string methodName, HandlerGraph parent)
        {
            var handlerType = typeof(T);
            var method = handlerType.GetMethod(methodName,
                BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

            if (method == null)
                throw new ArgumentOutOfRangeException(nameof(methodName),
                    $"Cannot find method named '{methodName}' in type {handlerType.FullName}");

            var call = new MethodCall(handlerType, method)
            {
                CommentText = "Core message handling method"
            };

            return new HandlerChain(call, parent);
        }

        internal MessageHandler CreateHandler(IContainer container)
        {
            if (_handlerType == null) throw new InvalidOperationException("The handler type has not been built yet");

            var handler = container.QuickBuild(_handlerType).As<MessageHandler>();
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
        internal List<Frame> DetermineFrames(GenerationRules rules, IContainer container)
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

        void ICodeFile.AssembleTypes(GeneratedAssembly assembly)
        {
            _generatedType = assembly.AddType(TypeName, typeof(MessageHandler));

            foreach (var handler in Handlers)
            {
                assembly.ReferenceAssembly(handler.HandlerType.Assembly);
            }

            var handleMethod = _generatedType.MethodFor(nameof(MessageHandler.Handle));
            handleMethod.Sources.Add(new MessageHandlerVariableSource(MessageType));
            handleMethod.Frames.AddRange(DetermineFrames(assembly.Rules, _parent.Container));

            handleMethod.DerivedVariables.Add(new Variable(typeof(Envelope),
                $"context.{nameof(IExecutionContext.Envelope)}"));

        }

        internal string SourceCode => _generatedType.SourceCode;

        Task<bool> ICodeFile.AttachTypes(GenerationRules rules, Assembly assembly, IServiceProvider services, string containingNamespace)
        {
            var found = this.As<ICodeFile>().AttachTypesSynchronously(rules, assembly, services, containingNamespace);
            return Task.FromResult(found);
        }

        bool ICodeFile.AttachTypesSynchronously(GenerationRules rules, Assembly assembly, IServiceProvider services,
            string containingNamespace)
        {
            _handlerType = assembly.ExportedTypes.FirstOrDefault(x => x.Name == TypeName);

            if (_handlerType == null) return false;

            Handler = (MessageHandler) services.As<IContainer>().QuickBuild(_handlerType);
            Handler.Chain = this;

            return true;
        }

        string ICodeFile.FileName => TypeName + ".cs";
    }
}
