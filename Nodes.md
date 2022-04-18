# Jasper & Marten notes

## Jasper

* Go ahead and introduce a base, marker interface called `Message`, then `Event` and `Command`. Strictly markers
* Add message discovery. `[JasperMessages]` for discovery???
* Log the listeners starting up
* Maybe do something like Marten's separate store registration for senders for very specific endpoints. Bypass the named endpoint or routing 

## Marten