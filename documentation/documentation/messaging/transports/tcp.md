<!--title:TCP Transport-->

<div class="alert alert-warning"><b>Note!</b> This transport works by sending traffic directly via sockets and may not be acceptable in your IT department policies. We are pursuing the usage of JWT's to secure the traffic between applications using the socket based transports, see <a href="https://github.com/JasperFx/jasper/issues/184">the GitHub issue</a></div>


## Lightweight, fire and forget

The TCP transport without durability is meant for scenarios where message delivery speed and throughput is important **and** guaranteed delivery is not required. We originally conceived this option as a .Net equivalent to [ZeroMQ](http://zeromq.org/).

To set up a Jasper application to listen for incoming and outgoing messages through the TCP transport in the lightweight mode, see this example:

<[sample:LightweightTransportApp]>


In the case of a failure to send a message, the lightweight transport will retry to send the message a few times (3 is the default), but the message will
be permanently discarded in about 10 seconds if it is unsuccessful. The lightweight transport is useful for control messages or messages that have
a very limited value in terms of time. My shop uses this transport for frequent status update messages that are very quickly obsolete.

## Durable TCP Messaging

First, see <[linkto:documentation/messaging/transports/durable]> about how message durability is enabled and functions within Jasper.

The TCP transport can be used durably as both listener or sender. To configure a durable TCP listener, use one of these options:

<[sample:DurableTransportApp]>

Note here that the Uri's `durable://server:port` and `tcp://server:port/durable` are considered as equivalents by Jasper.



