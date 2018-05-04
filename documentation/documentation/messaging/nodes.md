<!--title:Node Registration and Discovery-->

You may need Jasper to keep track of all the running nodes within a single logical system. For example, let's say that you're running your application in a Docker container hosted in Azure, and you have Azure configured to run up to 10 different nodes. 

Jasper provides the "node registration" feature to track all the running nodes in some kind of durable storage. Assuming there is a
registered service of this interface shown below other than the stand in, "in memory" default:

<[sample:INodeDiscovery]>

When a Jasper service node starts up successfully, it will try to persist information about itself to the `INodeDiscovery` service including:

* The service name
* The "node id" that is a combination of the service name and machine name
* Any local, listening Uri values. For example, if you specify that the service should listen for TCP messages at port 2222, you would have the value "tcp://[machine name]:2222"

Likewise, when a service node shuts down *successfully*, it will use the same service to remove itself from the node registry. 

Today, the existing options are to use:

1. <[linkto:documentation/extensions/consul]>
1. <[linkto:documentation/extensions/marten/subscriptions]>