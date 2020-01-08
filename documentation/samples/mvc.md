<!--title:Adding Jasper to an MVC Core Application-->

Jasper is trying very hard to be merely a citizen within the greater ASP.Net Core ecosystem than its own standalone framework. To that end, Jasper plays nicely with ASP.Net Core, as shown in this example that adds Jasper to an MVC Core application.

Start by creating a new project with this command line:

```
dotnet new webapi
```

Now, add a Nuget reference to Jasper. In your `Program.Main()` method, make these changes noted in comments:

<[sample:InMemoryMediatorProgram]>

Next, to see what other command line utilities Jasper has added to your project, type:

```
dotnet run -- help
```

And you'll see a lot of new options:

```
No Jasper extensions are detected
Searching 'Jasper, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' for commands

  ---------------------------------------------------------------------------------------------------
    Available commands:
  ---------------------------------------------------------------------------------------------------
    check-env -> Execute all environment checks against the application
     describe -> Writes out a description of your running application to either the console or a file
          run -> Runs the configured AspNetCore application
      storage -> Administer the envelope storage
  ----------------------------------------------------------------------------------------------------
```

The command line support in Jasper is really all from [Oakton.AspNetCore](https://jasperfx.github.io/oakton/documentation/aspnetcore/).

See <[linkto:bootstrapping/console]> for more information about Jasper's command line support.





