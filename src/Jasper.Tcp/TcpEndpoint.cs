using System;
using System.Net;
using Jasper.Configuration;
using Jasper.Runtime;
using Jasper.Transports;
using Jasper.Transports.Sending;
using Jasper.Util;

namespace Jasper.Tcp;

public class TcpEndpoint : Endpoint
{
    public TcpEndpoint() : this("localhost", 2000)
    {
    }

    public TcpEndpoint(int port) : this("localhost", port)
    {
    }

    public TcpEndpoint(string hostName, int port)
    {
        HostName = hostName;
        Port = port;

        // ReSharper disable once VirtualMemberCallInConstructor
        Name = Uri.ToString();
    }

    public override Uri Uri => ToUri(Port, HostName);

    public string HostName { get; private set; }

    public int Port { get; private set; }

    protected override bool supportsMode(EndpointMode mode)
    {
        return mode != EndpointMode.Inline;
    }

    public static Uri ToUri(int port, string hostName = "localhost")
    {
        return $"tcp://{hostName}:{port}".ToUri();
    }

    public override Uri CorrectedUriForReplies()
    {
        var uri = ToUri(Port, HostName);
        if (Mode != EndpointMode.Durable)
        {
            return uri;
        }

        return $"{uri}durable".ToUri();
    }

    public override void Parse(Uri uri)
    {
        if (uri.Scheme != "tcp")
        {
            throw new ArgumentOutOfRangeException(nameof(uri));
        }

        HostName = uri.Host;
        Port = uri.Port;

        if (uri.IsDurable())
        {
            Mode = EndpointMode.Durable;
        }
    }

    public override void StartListening(IJasperRuntime runtime)
    {
        if (!IsListener)
        {
            return;
        }

        var listener = createListener(runtime);
        runtime.Endpoints.AddListener(listener, this);
    }

    protected override ISender CreateSender(IJasperRuntime root)
    {
        return new BatchedSender(Uri, new SocketSenderProtocol(), root.Advanced.Cancellation, root.Logger);
    }

    private IListener createListener(IJasperRuntime root)
    {
        // check the uri for an ip address to bind to
        var cancellation = root.Advanced.Cancellation;

        var hostNameType = Uri.CheckHostName(HostName);

        if (hostNameType != UriHostNameType.IPv4 && hostNameType != UriHostNameType.IPv6)
        {
            return HostName == "localhost"
                ? new SocketListener(root.Logger, IPAddress.Loopback, Port, cancellation)
                : new SocketListener(root.Logger, IPAddress.Any, Port, cancellation);
        }

        var ipaddr = IPAddress.Parse(HostName);
        return new SocketListener(root.Logger, ipaddr, Port, cancellation);
    }
}
