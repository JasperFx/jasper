<!--title:IoC Container Integration-->

This is heavily in flight. It's StructureMap only *today*, but all the service registrations
are done with the ASP.Net Core DI abstractions, including a type scanning stolen out of
StructureMap. One of three things is going to happen before 1.0:

* We retain StructureMap, but only if StructureMap gets an overhaul for performance (there's
  overlap between the Jasper & StructureMap teams). Other option is that we effectively remove
  the IoC container from runtime message or HTTP processing in most cases so that the performance
  does not matter all that much
* We switch to using the built in ASP.Net DI container. So far, our experimentation on this one
  has not been positive
