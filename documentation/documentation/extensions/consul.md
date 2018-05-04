<!--title:Jasper.Consul-->


The Jasper.Consul extension is an integration with the outstanding [Consul](https://www.consul.io/) tool for service discovery and configuration. This extension uses the [ConsulDotNet](https://github.com/PlayFab/consuldotnet) library to read and write to Consul using its HTTP API.

## Uri Lookups

You can use the [Consul Key/Value store](https://www.consul.io/api/kv.html) for Uri aliasing in any place where Uri's are part of the service bus configuration. Here's 
an example:

<[sample:Using-Consul-Uri-Lookup]>

In the sample above, the Uri `consul://one` would refer to a `Uri` string in the key/value store with the key "one."

See <[linkto:documentation/messaging/configuration]> for more information about Uri lookups.

## Subscriptions

For <[linkto:documentation/messaging/routing/subscriptions;title=dynamic subscriptions]>, you can opt into using Consul as Jasper's [service discovery](https://en.wikipedia.org/wiki/Service_discovery) mechanism
by applying the `ConsulBackedSubscriptions` extension as shown below:

<[sample:AppUsingConsulBackedSubscriptions]>

The subscriptions are stored in Consul's key/value store using the pattern `jasper/subscription/[message-type]/[url encoded destination]`, and the value is a Json document reflecting the valid message representations for the subscriber.

## Node Discovery

<[linkto:documentation/messaging/nodes]> with Consul is automatically enabled if you have Jasper.Consul installed in your system.

## Customizing Consul Setup

By default, Jasper.Consul will connect to Consul at port 8500, but you can completely configure how the underlying ConsulDotNet `ConsulClient` will be built out like this:

<[sample:configuring-consul-in-jasperregistry]>

