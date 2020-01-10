<!--title:Message Json Schema Commands-->

The `Jasper.JsonCommands` extention adds a pair of commands to your application that allow you to export [JSON schema](http://json-schema.org/) documents from your handled message types or to generate C# [DTO](https://en.wikipedia.org/wiki/Data_transfer_object) classes to 
match JSON schema files. The purpose of this functionality is to avoid the necessity of having to share message types through some kind of shared DTO assembly in environments where that may be
problematic (or you just don't want to do that).

To use `Jasper.JsonCommands`, just add the Nuget to your project, and type `dotnet run -- ?` at the command line at the root of your project to see the list of available <[linkto:documentation/bootstrapping/console;title=commands]> in your Jasper application:

1. `export-json-schema` - Exports Json schema documents for all handled message types in the current application
1. `generate-message-types` - Generate C# classes from Json schema files exported by Jasper


## Exporting Json Schema

Assuming that you are using the `JasperHost` class as a harness for your Jasper application, you can use this command to export JSON schema files for all the known message types that your system handles:

```
dotnet run -- export-json-schema /schemafiles
```

That will loop through all the known messages handled in the system, and generate a JSON schema file
using the [NJsonSchema](https://github.com/RSuter/NJsonSchema) library that will be named following the pattern *<[linkto:documentation/messages;title=message type identity]>*-*version*.json. The argument "/schemafiles" just tells the command where to save the files.

See <[linkto:documentation/bootstrapping/console]> for more information on Jasper in command line applications.

## Generating C# DTO Class Files from JSON Schema Documents

To generate C# DTO class files from the JSON Schema documents, the `generate-message-types` command
has this usage:

```
 Usages for 'generate-message-types' (Generate C# classes from Json schema files exported by Jasper)
  generate-message-types <schemadirectory> <outputdirectory> [-n, --namespace <namespace>]

  -----------------------------------------------------------------------------------------------------
    Arguments
  -----------------------------------------------------------------------------------------------------
    schemadirectory -> The directory where the Json schema files are located
    outputdirectory -> The directory where the generated C# files should be written
  -----------------------------------------------------------------------------------------------------

  -----------------------------------------------------------------------------------------------------
    Flags
  -----------------------------------------------------------------------------------------------------
    [-n, --namespace <namespace>] -> Override the namespace of the generated message types
  -----------------------------------------------------------------------------------------------------
```

Let's say that the schema files were exported to a directory named `\\server1\exports` and I wanted to export the C# files to the directory `/Messages` within my project. Finally, I'd like these classes written to the namespace `MyApp.Messages` instead of the default "Jasper.Generated." That command would be:

```
dotnet run -- generate-message-types "\\server1\exports1" Messages --namespace MyApp.Messages
```

After running that command, you should see a .cs file per JSON schema document with generated C# code, and an additional files called `MessageAnnotations.cs` that has partial class declarations to add
the `[MessageAlias]` attribute to each type such that it matches the message type identity that the other applications expect.