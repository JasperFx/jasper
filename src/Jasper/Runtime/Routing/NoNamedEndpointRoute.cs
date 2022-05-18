using Baseline;
using Jasper.Transports.Sending;

namespace Jasper.Runtime.Routing;

internal class NoNamedEndpointRoute : IMessageRoute
{
    private readonly string _message;

    public NoNamedEndpointRoute(string endpointName, string[] allNames)
    {
        EndpointName = endpointName;

        var nameList = allNames.Join(", ");
        _message = $"Endpoint name '{endpointName}' is invalid. Known endpoints are {nameList}";
    }

    public Envelope CreateForSending(object message, DeliveryOptions? options, ISendingAgent localDurableQueue,
        JasperRuntime runtime)
    {
        throw new UnknownEndpointException(_message);
    }

    public string EndpointName { get; }
}