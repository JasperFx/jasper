using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Baseline;
using Baseline.Reflection;
using Jasper.Codegen;
using JasperBus.ErrorHandling;
using JasperBus.Runtime.Invocation;
using StructureMap;

namespace JasperBus.Model
{
    public class HandlerChain : IGenerates<MessageHandler>, IHasErrorHandlers
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
            if (messageType == null) throw new ArgumentNullException(nameof(messageType));

            MessageType = messageType;

            TypeName = messageType.FullName.Replace(".", "_").Replace("+", "_");
        }

        public string TypeName { get; }

        public List<MethodCall> Handlers = new List<MethodCall>();

        public IGenerationModel ToGenerationModel(IGenerationConfig config)
        {
            var frames = determineFrames();

            return new MessageHandlerGenerationModel(TypeName, MessageType, config, frames);
        }

        private List<Frame> determineFrames()
        {
            if (!Handlers.Any())
            {
                throw new InvalidOperationException("No method handlers configured for message type " + MessageType.FullName);
            }

            var configureMethods = Handlers.Select(x => x.HandlerType).Distinct()
                .Select(x => x.GetTypeInfo().GetMethod("Configure",
                    new Type[] {typeof(HandlerChain)}));

            foreach (var method in configureMethods)
            {
                method?.Invoke(null, new object[] {this});
            }

            foreach (var methodCall in Handlers.ToArray())
            {
                methodCall.Method.ForAttribute<ModifyHandlerChainAttribute>(att => att.Modify(this));
            }

            foreach (var handlerType in Handlers.Select(x => x.HandlerType).Distinct().ToArray())
            {
                handlerType.ForAttribute<ModifyHandlerChainAttribute>(att => att.Modify(this));
            }

            var i = 0;
            var cascadingHandlers = Handlers.Where(x => x.ReturnVariable != null)
                .Select(x => new CaptureCascadingMessages(x.ReturnVariable, ++i));

            return Wrappers.Concat(Handlers).Concat(cascadingHandlers).ToList();
        }

        private HandlerChain(MethodCall @call) : this(@call.Method.MessageType())
        {
            Handlers.Add(@call);
        }

        public HandlerChain(IGrouping<Type, HandlerCall> grouping) : this(grouping.Key)
        {
            Handlers.AddRange(grouping);
        }

        public string SourceCode { get; set; }
        public int MaximumAttempts { get; set; } = 1;
        public IList<IErrorHandler> ErrorHandlers { get; } = new List<IErrorHandler>();

        MessageHandler IGenerates<MessageHandler>.Create(Type[] types, IContainer container)
        {
            var type = types.FirstOrDefault(x => x.Name == TypeName);
            if (type == null)
            {
                throw new ArgumentOutOfRangeException(nameof(types), $"Could not find a type named '{TypeName}' in this assembly");
            }

            var handler = container.GetInstance(type).As<MessageHandler>();

            handler.Chain = this;

            return handler;
        }

        public void AddAbstractedHandler(HandlerCall call)
        {
            var clone = call.Clone(MessageType);

            Handlers.Add(clone);
        }

        public readonly IList<Frame> Wrappers = new List<Frame>();

        public override string ToString()
        {
            return $"{MessageType.Name} handled by {Handlers.Select(x => $"{x.HandlerType.Name}.{x.Method.Name}()").Join(", ")}";
        }

        public GeneratedClass ToClass(IGenerationConfig config)
        {
            var @class = new GeneratedClass(config, TypeName)
            {
                BaseType = typeof(MessageHandler)
            };

            var method = new HandleMessageMethod(determineFrames());
            method.Sources.Add(new MessageHandlerVariableSource(MessageType));

            @class.AddMethod(method);

            return @class;
        }
    }

    public class HandleMessageMethod : GeneratedMethod
    {
        public HandleMessageMethod(IList<Frame> frames) : base(nameof(MessageHandler.Handle), new Argument[] { Argument.For<IInvocationContext>("context") }, frames)
        {
            Overrides = true;
        }
    }
}