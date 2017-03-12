using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Baseline;
using Baseline.Reflection;
using Jasper.Codegen;
using StructureMap;

namespace JasperBus.Model
{
    public class HandlerChain : IGenerates<MessageHandler>
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

            TypeName = messageType.FullName.Replace(".", "_");
        }

        public string TypeName { get; }

        public List<MethodCall> Handlers = new List<MethodCall>();


        public IGenerationModel ToGenerationModel(IGenerationConfig config)
        {
            if (!Handlers.Any())
            {
                throw new InvalidOperationException("No method handlers configured for message type " + MessageType.FullName);
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

            var frames = Wrappers.Concat(Handlers).Concat(cascadingHandlers).ToList();

            return new MessageHandlerGenerationModel(TypeName, MessageType, config, frames);
        }

        private HandlerChain(MethodCall @call) : this(@call.Method.MessageType())
        {
            Handlers.Add(@call);
        }

        public HandlerChain(IGrouping<Type, HandlerCall> grouping) : this(grouping.Key)
        {
            Handlers.AddRange(grouping);
        }

        string IGenerates<MessageHandler>.SourceCode { get; set; }

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
    }
}