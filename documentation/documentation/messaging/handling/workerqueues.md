<!--title:Worker Queues and Message Priority-->

<div class="alert alert-info"><b>Note!</b> The "worker queues" are completely independent from the transports. All of the built in transport types (loopback, tcp, or http) enqueue received messages into the local worker queues.</div>

To provide more fine-grained control over how messages are handled within your application, you can assign messages to 
named "worker queues" within the application that have configurable priority through a maximum number of concurrent threads handling messages within that worker queue. 

Also, the worker queues can optionally be designated as "durable," meaning that local, loopback messages would be persisted until they can be successfully processed to provide a guarantee that the message will be successfully processed in the case of the running application faulting or having been shut down prematurely (assuming that other nodes are running or it's restarted later of course).

See <[linkto:documentation/messaging/transports/durable]> and <[linkto:documentation/messaging/transports/loopback]> for more information.

Worker queue assignment and configuration can be made through the `JasperRegistry` fluent interface like so:

<[sample:AppWithWorkerQueues]>



<div class="alert alert-info"><b>Note!</b> By default, any incoming message would be handled within a "default" worker queue with a maximum thread count of 5 and local messages would not be durable.</div>

Alternatively, you can create a worker queue for a specific message type by using the `[Worker]` attribute on a message
type as shown below:

<[sample:using-WorkerAttribute]>

This attribute will configure the worker queue with the designated queue name, parallelization, and durability. Do note that in the event of a name conflict between the attributes and named worker queues in the `JasperRegistry` class for the system, the attributes are processed last and would win out. 



