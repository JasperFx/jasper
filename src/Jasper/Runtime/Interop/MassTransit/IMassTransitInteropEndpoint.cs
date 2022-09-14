using System;

namespace Jasper.Runtime.Interop.MassTransit;

public interface IMassTransitInteropEndpoint
{
    Uri? ToMassTransitUri();
}
