# Jasper & Marten notes

## Jasper

* Go ahead and introduce a base, marker interface called `Message`, then `Event` and `Command`. Strictly markers
* Add message discovery. `[JasperMessages]` for discovery???
* Log the listeners starting up
* Maybe do something like Marten's separate store registration for senders for very specific endpoints. Bypass the named endpoint or routing 
* Register some kind of health checks?
* Standardize on some kind of `Use[TransportName]()` extension method directly on `JasperOptions`. Be consistent across the board.
* *Might* consider using a connection string approach for Rabbit MQ. Shamelessly rip off the one from NServiceBus
* MT lets you limit the concurrency on a single message type regardless of endpoint. Don't think we want that one
* Definitely keep the inbox/outbox at the endpoint level.
* Go back to the idea of an admin endpoint. Automatic "fan out" to each. Maybe make "FanOutToEachNode()" as part of routing so that it can imply 
  that semantics. 
* Think the saga and transactional code generation needs to be a bit more flexible. Like it needs to "see" what the dependencies are first and adapt


### Jasper on HTTP 

* Figure out how to move attributes on Handler methods to the ASP.Net Core `Endpoint`?

## Marten