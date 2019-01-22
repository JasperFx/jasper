<!--title:Jasper.MvcExtender-->

<[warning]>
This extension is brand spanking new, and only supports a subset of MVC Core. It would be very, very helpful to have some early adopters
kick the tires on this.
<[/warning]>

Rather than reinvent the whole MVC Core world, Jasper has an extension library called *Jasper.MvcExtender* that allows you to use a subset of MVC Core artifacts within Jasper endpoints and even to use MVC Core `Controller` classes within Jasper's more efficient runtime pipeline. The only thing you need to do to utilize this add on is to make the Nuget reference and Jasper will automatically discover the extension and apply it to your Jasper configuration.

See <[linkto:documentation/extensions]> for more information about how Jasper extensions work.


## Controller and ControllerBase

The presence of the *Jasper.MvcExtender* extension allows Jasper to automatically discover endpoint actions on concrete classes that
inherit from MVC Core's `ControllerBase` type. The action methods can either follow Jasper's idiomatic routing naming convention like this example:

<[sample:ControllerUsingJasperRouting]>

Or, you can use the MVC Core attributes for expressing the routing patterns and Jasper will consider any method marked with one of these attributes as an HTTP action method. Here's an example:

<[sample:ControllerUsingMvcRouting]>

Lastly, Jasper has limited support for the `[Route]` attribute like this:

<[sample:UsingRouteAttribute]>

Lastly, the `ControllerBase.HttpContext` property is set during the course of executing a `ControllerBase` action as shown here:

<[sample:using-HttpContext-in-Controller]>


## IActionResult

With the *Jasper.MvcExtender* added to your project, Jasper can use [IActionResult](https://docs.microsoft.com/en-us/aspnet/core/web-api/action-return-types?view=aspnetcore-2.2) objects as the resource type like this:

<[sample:using-IActionResult]>




