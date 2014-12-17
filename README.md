jasper
======

Next generation application development framework for .Net. Jasper is being built on the new ASP.Net vNext platform as the successor to [FubuMVC](https://github.com/DarthFubuMVC/fubumvc).

Goals
=====
* Provide a relatively easy migration path from existing FubuMVC applications
* Require minimal ceremonial code and cruft
* Act as a new foundation for FubuTransportation in place of FubuMVC
* High performance and scalability
* Optimized application startup
* At least match the modularity and extensibility of FubuMVC + Bottles


Architecture
============
* "Russian Doll" model
* Use OWIN AppFunc as the new "Behavior", which effectively means "async by default"
* Build on top of the new K runtime
* Heavily leverage Roslyn capabilities
* Use StructureMap 3.1+ as the default IoC container


Components
==========

The hope is that Jasper itself can be very modular such that users could opt into or out of the various components and even use some of the Jasper components within ASP.Net MVC vnext applications.

1. Jasper.Routing -- A new router for OWIN hosted applications based on the Trie algorithm
2. Jasper.Routing.Model -- Add reverse url lookup by HTTP method and input model or action method
2. Jasper.Owin -- Helpers for working with the raw OWIN environment dictionary. 
3. Jasper.Composer -- Semantic model for composing OWIN handlers into "chains", then baking into a single OWIN AppFunc per route
4. Jasper.Transportation -- A port of FubuTransportation to the new Jasper architecture
5. Jasper.Binding -- Replacement for FubuCore model binding. The intention is to heavily leverage Roslyn metaprogramming instead of FubuCore's reflection heavy approach
6. Jasper.Hypermedia -- dunno exactly what this would be yet but it was in my notes;)
7. jasper -- command line tool for development that would hopefully combine the very best of Scala's SBT (but faster) and a new auto test feature wrapped around Fixie and heavily inspired by Goconvey.


