<!--title:Jasper Command Line Support-->

<[info]>
"Oakton" is a small community just to the North of Jasper's namesake.
<[/info]>

Jasper uses the related [Oakton](https://jasperfx.github.io/oakton) projects for command line parsing and its command runner extensibility. The extension method `RunJasper(args)` off of `IHostBuilder` is really just a call to [Oakton.AspNetCore command execution](https://jasperfx.github.io/oakton/documentation/aspnetcore/) that was kept for backward compatibility.

On top of the built in commands from Oakton.AspNetCore for executing the application and running [environment checks](https://jasperfx.github.io/oakton/documentation/aspnetcore/environment/), Jasper adds a new command called `storage` for managing any configured message configuration. See <[linkto:documentation/durability]> for more information on this command.

Custom Oakton commands will automatically be discovered from any loaded <[linkto:documentation/extensions;title=Jasper extensions]>.