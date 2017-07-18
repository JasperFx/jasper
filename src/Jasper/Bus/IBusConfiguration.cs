using System;
using System.Reflection;
using Jasper.Bus.Configuration;
using Jasper.Bus.ErrorHandling;

namespace Jasper.Bus
{
    public interface IBusConfiguration
    {
        HandlerSource Handlers { get; }
        Policies Policies { get; }
        JasperBusRegistry.SerializationExpression Serialization { get; }
        string ServiceName { get; set; }
        IHasErrorHandlers ErrorHandling { get; }
        ChannelExpression ListenForMessagesFrom(Uri uri);
        ChannelExpression ListenForMessagesFrom(string uriString);
        JasperBusRegistry.SendExpression SendMessage<T>();
        JasperBusRegistry.SendExpression SendMessages(string description, Func<Type, bool> filter);
        JasperBusRegistry.SendExpression SendMessagesInNamespace(string @namespace);
        JasperBusRegistry.SendExpression SendMessagesInNamespaceContaining<T>();
        JasperBusRegistry.SendExpression SendMessagesFromAssembly(Assembly assembly);
        JasperBusRegistry.SendExpression SendMessagesFromAssemblyContaining<T>();
        JasperBusRegistry.SubscriptionExpression SubscribeAt(string receiving);
        JasperBusRegistry.SubscriptionExpression SubscribeAt(Uri receiving);
        JasperBusRegistry.SubscriptionExpression SubscribeLocally();
        ChannelExpression Channel(Uri uri);
        ChannelExpression Channel(string uriString);
    }
}