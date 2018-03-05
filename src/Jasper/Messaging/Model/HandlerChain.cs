using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Baseline;
using Baseline.Reflection;
using Jasper.Configuration;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.ErrorHandling;
using Jasper.Messaging.Runtime;
using Lamar;
using Lamar.Codegen;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;
using Lamar.Compilation;

namespace Jasper.Messaging.Model
{
    public class HandlerChain : Chain<HandlerChain, ModifyHandlerChainAttribute>, IHasErrorHandlers
    {
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
            {
                throw new ArgumentOutOfRangeException(nameof(methodName), $"Cannot find method named '{methodName}' in type {handlerType.FullName}");
            }

            var call = new MethodCall(handlerType, method);

            return new HandlerChain(call);
        }

        public Type MessageType { get; }

        public HandlerChain(Type messageType)
        {
            MessageType = messageType ?? throw new ArgumentNullException(nameof(messageType));

            TypeName = messageType.FullName.Replace(".", "_").Replace("+", "_");
        }

        public string TypeName { get; }

        public readonly List<MethodCall> Handlers = new List<MethodCall>();
        private GeneratedType _generatedType;

        public void AssembleType(GeneratedAssembly generatedAssembly)
        {
            _generatedType = generatedAssembly.AddType(TypeName, typeof(MessageHandler));
            var handleMethod = _generatedType.MethodFor(nameof(MessageHandler.Handle));
            handleMethod.Sources.Add(new MessageHandlerVariableSource(MessageType));
            handleMethod.Frames.AddRange(DetermineFrames());

            handleMethod.DerivedVariables.Add(new Variable(typeof(Envelope), $"context.{nameof(IMessageContext.Envelope)}"));
            handleMethod.DerivedVariables.Add(new Variable(typeof(IAdvancedMessagingActions), $"context.{nameof(IMessageContext.Advanced)}"));
        }

        public MessageHandler CreateHandler(IContainer container)
        {
            var handler = container.QuickBuild(_generatedType.CompiledType).As<MessageHandler>();
            handler.Chain = this;

            return handler;
        }

        public List<Frame> DetermineFrames()
        {
            if (!Handlers.Any())
            {
                throw new InvalidOperationException("No method handlers configured for message type " + MessageType.FullName);
            }

            applyAttributesAndConfigureMethods();

            foreach (var attribute in MessageType.GetTypeInfo().GetCustomAttributes(typeof(ModifyHandlerChainAttribute)).OfType<ModifyHandlerChainAttribute>())
            {
                attribute.Modify(this);
            }

            foreach (var attribute in MessageType.GetTypeInfo().GetCustomAttributes(typeof(ModifyChainAttribute)).OfType<ModifyChainAttribute>())
            {
                attribute.Modify(this);
            }

            var i = 0;
            var cascadingHandlers = Handlers.Where(x => x.ReturnVariable != null)
                .Select(x => new CaptureCascadingMessages(x.ReturnVariable, ++i));

            return Middleware.Concat(Handlers).Concat(cascadingHandlers).ToList();
        }

        protected override MethodCall[] handlerCalls()
        {
            return Handlers.ToArray();
        }


        private HandlerChain(MethodCall @call) : this(@call.Method.MessageType())
        {
            Handlers.Add(@call);
        }

        public HandlerChain(IGrouping<Type, HandlerCall> grouping) : this(grouping.Key)
        {
            Handlers.AddRange(grouping);
        }

        public string SourceCode => _generatedType.SourceCode;
        public int MaximumAttempts { get; set; } = 1;
        public IList<IErrorHandler> ErrorHandlers { get; } = new List<IErrorHandler>();


        public void AddAbstractedHandler(HandlerCall call)
        {
            var clone = call.Clone(MessageType);

            Handlers.Add(clone);
        }

        public override string ToString()
        {
            return $"{MessageType.Name} handled by {Handlers.Select(x => $"{x.HandlerType.Name}.{x.Method.Name}()").Join(", ")}";
        }



    }

}
