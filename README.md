jasper
======

Next generation application development framework for .Net. Jasper is being built on the new DNX platform as the successor to [FubuMVC](https://github.com/DarthFubuMVC/fubumvc) and [FubuTransportation](https://github.com/DarthFubuMVC/fubutransportation).

Goals
=====
* Provide a relatively easy migration path from existing FubuMVC/FubuTransportation applications
* Require minimal ceremonial code and cruft
* High performance and scalability
* Optimized application startup
* At least match the modularity and extensibility of current FubuMVC + Bottles


Architecture
============
* Keep the "Russian Doll" model
  * A pipeline chain of "behaviors" (much like middleware in [OWIN](http://owin.org/))
* Combine FubuMVC and FubuTransportation into one library
* Use OWIN AppFunc as the new "Behavior", which effectively means "async by default"
* Build on top of the new DNX .Net runtime. Hopefully shoot for CoreCLR support
* Heavily leverage Roslyn capabilities
* Use StructureMap 3.2+ as the default IoC container


