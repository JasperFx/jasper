# Jasper & Marten notes

## Jasper

* Go ahead and introduce a base, marker interface called `Message`, then `Event` and `Command`. Strictly markers
* Add message discovery. `[JasperMessages]` for discovery???
* Log the listeners starting up
* Maybe do something like Marten's separate store registration for senders for very specific endpoints. Bypass the named endpoint or routing 
* Register some kind of health checks?
* Standardize on some kind of `Use[TransportName]()` extension method directly on `JasperOptions`. Be consistent across the board.


### Jasper on HTTP 

* Figure out how to move attributes on Handler methods to the ASP.Net Core `Endpoint`?

## Marten