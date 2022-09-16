using System;

namespace Jasper.Runtime.Interop.MassTransit;

public interface IMassTransitInteropEndpoint
{
    Uri? MassTransitUri();
    Uri? MassTransitReplyUri();

    Uri? TranslateMassTransitToJasperUri(Uri uri);
}
