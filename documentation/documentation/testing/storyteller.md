<!--title:Using Storyteller against Jasper Systems-->

Jasper comes with a pre-built recipe for doing integration or acceptance testing with [Storyteller](http://storyteller.github.io) using
the *Jasper.Storyteller* extension library.

To get started with this package, create a new console application in your solution and add the `Jasper.Storyteller` Nuget dependency. Next,
in the `Program.Main()` method, use this code to connect your application to Storyteller:

<[sample:bootstrapping-storyteller-with-Jasper]>

In this case, `MyJasperAppRegistry` would be the name of whatever the `JasperRegistry` class is for your application.

## MessagingFixture

TODO -- more stuff here.

