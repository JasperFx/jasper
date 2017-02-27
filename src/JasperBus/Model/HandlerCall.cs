using System;
using System.Linq.Expressions;
using System.Reflection;
using Baseline;
using Baseline.Reflection;
using Jasper.Codegen;

namespace JasperBus.Model
{
    public class HandlerCall : MethodCall
    {
        public static HandlerCall For<T>(Expression<Action<T>> method)
        {
            return new HandlerCall(typeof(T), ReflectionHelper.GetMethod(method));
        }

        public HandlerCall(Type handlerType, MethodInfo method) : base(handlerType, method)
        {
            MessageType = method.MessageType();
        }

        public Type MessageType { get; }
        public Type ActualMessageType { get; internal set; }

        public bool CouldHandleOtherMessageType(Type messageType)
        {
            if (messageType == MessageType) return false;

            return messageType.CanBeCastTo(MessageType);
        }

        public HandlerCall Clone()
        {
            return new HandlerCall(HandlerType, Method);
        }
    }
}