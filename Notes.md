# Jasper & Marten notes

## Jasper

* Go ahead and introduce a base, marker interface called `Message`, then `Event` and `Command`. Strictly markers
* Add message discovery. `[JasperMessages]` for discovery???
* Log the listeners starting up
* Maybe do something like Marten's separate store registration for senders for very specific endpoints. Bypass the named endpoint or routing 
* Register some kind of health checks? Research `IServiceCollection.AddHealthChecks()`
* Standardize on some kind of `Use[TransportName]()` extension method directly on `JasperOptions`. Be consistent across the board.
* *Might* consider using a connection string approach for Rabbit MQ. Shamelessly rip off the one from NServiceBus
* MT lets you limit the concurrency on a single message type regardless of endpoint. Don't think we want that one
* Definitely keep the inbox/outbox at the endpoint level.
* Go back to the idea of an admin endpoint. Automatic "fan out" to each. Maybe make "FanOutToEachNode()" as part of routing so that it can imply 
  that semantics. "Control Bus" -- bring back support from FubuTransportation
* Think the saga and transactional code generation needs to be a bit more flexible. Like it needs to "see" what the dependencies are first and adapt
* May have to use pluggable message type name rules to work with MassTransit. Might just steal their code.
* For Rabbit MQ, assume a routing rule where everything is sent to a routing key & fan out exchange with the message type name. Routing rule will need to 
  be able to alter an `ITransport` on the JasperOptions. Or plug in routing rules directly that can also register endopints. Think that's actually possible
* Rollback commands? Think about this.
* Really need the circuit breaker
* Inbox/outbox should guard against duplicate. Enforce idempotency especially at the incoming side of things

### Rabbit MQ

* Tie the Envelope.DeliverBy prop to Rabbit MQ's TTL???
* More queues for throughput
* Use temporary queue for control messages. Also make auto-delete and exclusive (later)
* Surface priority in rabbit mq config within jasper?
* Share a connection, but not the channels. Channels cannot be shared between threads, not that we do. One connection for incoming, and one for outgoing?
* do a little more research on `x-dead-letter-exchange` and `x-dead-letter-routing-key` for dead lettering messages in rabbit mq
* Enable prefetch count and size configuration on Rabbit Mq endpoint. You set it w/ Channel.BasicQos() 
* See the [retry queue idea](https://nordvpn.com/blog/rabbitmq-use-cases/). Maybe a better way of doing delayed retries. Also really important to get a circuit breaker in place.
* The buffered listener approach will need backpressure to shut off the listener when the in memory count gets too high
* Might invest some time in strong typed helpers on queue arguments for things like `"x-queue-mode", "lazy"`

### Doc Notes

* Play up the concurrency support, sample producer/consumer model would be nice
* communication between microservices, local command bus for easy concurrency, CQRS
* Talk about Event vs Command
* eliminating temporal coupling, plus error handling
* Show a sequence diagram of how a command/message gets to its handler
* Talk about Command Dispatcher pattern
* Command routing. Mention that Jasper routes based on the concrete type of the message
* Advantages of Jasper's Russian Doll model



### Jasper on HTTP 

* Figure out how to move attributes on Handler methods to the ASP.Net Core `Endpoint`?

## Marten
