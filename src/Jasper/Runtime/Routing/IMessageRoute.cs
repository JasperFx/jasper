using Jasper.Transports.Sending;

namespace Jasper.Runtime.Routing;

internal interface IMessageRoute
{
    Envelope CreateForSending(object message, DeliveryOptions? options, ISendingAgent localDurableQueue,
        JasperRuntime runtime);
}