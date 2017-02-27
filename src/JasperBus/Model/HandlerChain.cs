using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Baseline.Reflection;
using Jasper.Codegen;
using Jasper.Internal;
using JasperBus.Runtime.Invocation;
using StructureMap;

namespace JasperBus.Model
{
    public abstract class MessageHandler : IHandler<IInvocationContext>
    {
        public HandlerChain Chain { get; set; }

        public abstract Task Handle(IInvocationContext input);
    }


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

        HandlerCode IGenerates<MessageHandler>.ToHandlerCode()
        {
            if (!Handlers.Any())
            {
                throw new InvalidOperationException("No method handlers configured for message type " + MessageType.FullName);
            }

            var chain = new HandlerCode(TypeName, typeof(MessageHandler));

            foreach (var method in Handlers)
            {
                chain.AddToEnd(method);
            }

            return chain;
        }

        private string _code;

        private HandlerChain(MethodCall @call) : this(@call.Method.MessageType())
        {
            Handlers.Add(@call);
        }

        string IGenerates<MessageHandler>.SourceCode
        {
            get { return _code; }
            set { _code = value; }
        }

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
    }
}