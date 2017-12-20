<!--title:TCP Transport-->

<div class="alert alert-info"><b>Note!</b> The lightweight transport was conceived as a .Net equivalent to <a href="http://zeromq.org/">ZeroMQ</a></div>

<div class="alert alert-warning"><b>Note!</b> This transport works by sending traffic directly via sockets and may not be acceptable in your IT department policies. We are pursuing the usage of JWT's to secure the traffic between applications using the socket based transports, see <a href="https://github.com/JasperFx/jasper/issues/184">the GitHub issue</a></div>


The lightweight transport is meant for scenarios where message delivery speed and throughput is important **and** guaranteed delivery is not required.
In the case of a failure to send a message, the lightweight transport will retry to send the message a few times (3 is the default), but the message will
be permanently discarded in about 10 seconds if it is unsuccessful. The lightweight transport is useful for control messages or messages that have
a very limited value in terms of time. My shop uses this transport for frequent status update messages that are very quickly obsolete.

To use the lightweight transport, here are examples of all the common use cases:

<[sample:LightweightTransportApp]>