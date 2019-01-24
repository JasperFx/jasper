<!--title:Hybrid Jasper/MVC Core Application-->

Jasper is trying very hard to be merely a citizen within the greater ASP.Net Core ecosystem than its own standalone framework. To that end, Jasper plays nicely with ASP.Net Core, as shown in this example that adds Jasper to an MVC Core application.

Start by creating a new project with this command line:

```
dotnet new webapi
```

Now, add a Nuget reference to Jasper. In your `Program.Main()` method, make these changes noted in comments:

<[sample:MvcCoreHybrid.Program]>

Next, go into the generated `Startup` class and make the changes shown here with comments:

<[sample:MvcCoreHybrid.Startup]>

In this case, I'm assuming that there's one single messaging listener and all published messages will get
sent to the same location. Both of those Uri values are in the `appsettings.json` file shown below:

```
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ListeningEndoint": "tcp://localhost:2100",
  "OtherServiceUri": "tcp://localhost:2200"
}

```

Now, after running the application from a straight up call to `dotnet run` (and maybe remembering to force your csproj to copy the `appsettings.json` file around), you should get a lot of command line output like this:

```
dbug: Microsoft.AspNetCore.Hosting.Internal.WebHost[3]
      Hosting starting
info: Microsoft.AspNetCore.DataProtection.KeyManagement.XmlKeyManager[0]
      User profile is available. Using '/Users/jeremydmiller/.aspnet/DataProtection-Keys' as key repository; keys will not be encrypted at rest.
dbug: Microsoft.AspNetCore.DataProtection.Repositories.FileSystemXmlRepository[37]
      Reading data from file '/Users/jeremydmiller/.aspnet/DataProtection-Keys/key-fdd38e0a-635d-4d73-93bf-7197b3bf71a8.xml'.
dbug: Microsoft.AspNetCore.DataProtection.Repositories.FileSystemXmlRepository[37]
      Reading data from file '/Users/jeremydmiller/.aspnet/DataProtection-Keys/key-506cdf5d-957b-4bdd-8b07-3edf5a109e9a.xml'.
dbug: Microsoft.AspNetCore.DataProtection.Repositories.FileSystemXmlRepository[37]
      Reading data from file '/Users/jeremydmiller/.aspnet/DataProtection-Keys/key-122b0ab4-2cb0-49ac-ae79-98a3ca730514.xml'.
dbug: Microsoft.AspNetCore.DataProtection.KeyManagement.XmlKeyManager[18]
      Found key {fdd38e0a-635d-4d73-93bf-7197b3bf71a8}.
dbug: Microsoft.AspNetCore.DataProtection.KeyManagement.XmlKeyManager[18]
      Found key {506cdf5d-957b-4bdd-8b07-3edf5a109e9a}.
dbug: Microsoft.AspNetCore.DataProtection.KeyManagement.XmlKeyManager[18]
      Found key {122b0ab4-2cb0-49ac-ae79-98a3ca730514}.
dbug: Microsoft.AspNetCore.DataProtection.KeyManagement.DefaultKeyResolver[13]
      Considering key {fdd38e0a-635d-4d73-93bf-7197b3bf71a8} with expiration date 2019-02-26 20:28:23Z as default key.
dbug: Microsoft.AspNetCore.DataProtection.TypeForwardingActivator[0]
      Forwarded activator type request from Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel.AuthenticatedEncryptorDescriptorDeserializer, Microsoft.AspNetCore.DataProtection, Version=2.1.1.0, Culture=neutral, PublicKeyToken=adb9793829ddae60 to Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel.AuthenticatedEncryptorDescriptorDeserializer, Microsoft.AspNetCore.DataProtection, Culture=neutral, PublicKeyToken=adb9793829ddae60
dbug: Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ManagedAuthenticatedEncryptorFactory[11]
      Using managed symmetric algorithm 'System.Security.Cryptography.Aes'.
dbug: Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ManagedAuthenticatedEncryptorFactory[10]
      Using managed keyed hash algorithm 'System.Security.Cryptography.HMACSHA256'.
dbug: Microsoft.AspNetCore.DataProtection.KeyManagement.KeyRingProvider[2]
      Using key {fdd38e0a-635d-4d73-93bf-7197b3bf71a8} as the default key.
dbug: Microsoft.AspNetCore.DataProtection.Internal.DataProtectionStartupFilter[0]
      Key ring with default key {fdd38e0a-635d-4d73-93bf-7197b3bf71a8} was loaded during application startup.
dbug: Microsoft.AspNetCore.Mvc.MvcJsonOptions[0]
      Compatibility switch AllowInputFormatterExceptionMessages in type MvcJsonOptions is using compatibility value True for version Version_2_2
dbug: Microsoft.AspNetCore.Mvc.MvcOptions[0]
      Compatibility switch AllowCombiningAuthorizeFilters in type MvcOptions is using compatibility value True for version Version_2_2
dbug: Microsoft.AspNetCore.Mvc.MvcOptions[0]
      Compatibility switch AllowBindingHeaderValuesToNonStringModelTypes in type MvcOptions is using compatibility value True for version Version_2_2
dbug: Microsoft.AspNetCore.Mvc.MvcOptions[0]
      Compatibility switch AllowValidatingTopLevelNodes in type MvcOptions is using compatibility value True for version Version_2_2
dbug: Microsoft.AspNetCore.Mvc.MvcOptions[0]
      Compatibility switch InputFormatterExceptionPolicy in type MvcOptions is using compatibility value MalformedInputExceptions for version Version_2_2
dbug: Microsoft.AspNetCore.Mvc.MvcOptions[0]
      Compatibility switch SuppressBindingUndefinedValueToEnumType in type MvcOptions is using compatibility value True for version Version_2_2
dbug: Microsoft.AspNetCore.Mvc.MvcOptions[0]
      Compatibility switch EnableEndpointRouting in type MvcOptions is using compatibility value True for version Version_2_2
dbug: Microsoft.AspNetCore.Mvc.MvcOptions[0]
      Compatibility switch MaxValidationDepth in type MvcOptions is using compatibility value 32 for version Version_2_2
dbug: Microsoft.AspNetCore.Mvc.MvcOptions[0]
      Compatibility switch AllowShortCircuitingValidationWhenNoValidatorsArePresent in type MvcOptions is using compatibility value True for version Version_2_2
dbug: Microsoft.AspNetCore.Mvc.ApiBehaviorOptions[0]
      Compatibility switch SuppressMapClientErrors in type ApiBehaviorOptions is using default value False
dbug: Microsoft.AspNetCore.Mvc.ApiBehaviorOptions[0]
      Compatibility switch SuppressUseValidationProblemDetailsForInvalidModelStateResponses in type ApiBehaviorOptions is using default value False
dbug: Microsoft.AspNetCore.Mvc.ApiBehaviorOptions[0]
      Compatibility switch AllowInferringBindingSourceForCollectionTypesAsFromQuery in type ApiBehaviorOptions is using default value False
dbug: Microsoft.AspNetCore.Mvc.RazorPages.RazorPagesOptions[0]
      Compatibility switch AllowAreas in type RazorPagesOptions is using compatibility value True for version Version_2_2
dbug: Microsoft.AspNetCore.Mvc.RazorPages.RazorPagesOptions[0]
      Compatibility switch AllowMappingHeadRequestsToGetHandler in type RazorPagesOptions is using compatibility value True for version Version_2_2
dbug: Microsoft.AspNetCore.Mvc.RazorPages.RazorPagesOptions[0]
      Compatibility switch AllowDefaultHandlingForOptionsRequests in type RazorPagesOptions is using compatibility value True for version Version_2_2
dbug: Microsoft.AspNetCore.Mvc.Razor.RazorViewEngineOptions[0]
      Compatibility switch AllowRecompilingViewsOnFileChange in type RazorViewEngineOptions is using explicitly configured value True
dbug: Microsoft.AspNetCore.Mvc.MvcViewOptions[0]
      Compatibility switch SuppressTempDataAttributePrefix in type MvcViewOptions is using compatibility value True for version Version_2_2
dbug: Microsoft.AspNetCore.Mvc.MvcViewOptions[0]
      Compatibility switch AllowRenderingMaxLengthAttribute in type MvcViewOptions is using compatibility value True for version Version_2_2
dbug: Microsoft.AspNetCore.Mvc.ModelBinding.ModelBinderFactory[12]
      Registered model binder providers, in the following order: Microsoft.AspNetCore.Mvc.ModelBinding.Binders.BinderTypeModelBinderProvider, Microsoft.AspNetCore.Mvc.ModelBinding.Binders.ServicesModelBinderProvider, Microsoft.AspNetCore.Mvc.ModelBinding.Binders.BodyModelBinderProvider, Microsoft.AspNetCore.Mvc.ModelBinding.Binders.HeaderModelBinderProvider, Microsoft.AspNetCore.Mvc.ModelBinding.Binders.FloatingPointTypeModelBinderProvider, Microsoft.AspNetCore.Mvc.ModelBinding.Binders.EnumTypeModelBinderProvider, Microsoft.AspNetCore.Mvc.ModelBinding.Binders.SimpleTypeModelBinderProvider, Microsoft.AspNetCore.Mvc.ModelBinding.Binders.CancellationTokenModelBinderProvider, Microsoft.AspNetCore.Mvc.ModelBinding.Binders.ByteArrayModelBinderProvider, Microsoft.AspNetCore.Mvc.ModelBinding.Binders.FormFileModelBinderProvider, Microsoft.AspNetCore.Mvc.ModelBinding.Binders.FormCollectionModelBinderProvider, Microsoft.AspNetCore.Mvc.ModelBinding.Binders.KeyValuePairModelBinderProvider, Microsoft.AspNetCore.Mvc.ModelBinding.Binders.DictionaryModelBinderProvider, Microsoft.AspNetCore.Mvc.ModelBinding.Binders.ArrayModelBinderProvider, Microsoft.AspNetCore.Mvc.ModelBinding.Binders.CollectionModelBinderProvider, Microsoft.AspNetCore.Mvc.ModelBinding.Binders.ComplexTypeModelBinderProvider
dbug: Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServer[2]
      Failed to locate the development https certificate at '(null)'.
dbug: Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServer[0]
      Using development certificate: CN=localhost (Thumbprint: 81C087389BE208DFD502D193565DB7503A96E805)
dbug: Microsoft.AspNetCore.Server.Kestrel[0]
      No listening endpoints were configured. Binding to http://localhost:5000 and https://localhost:5001 by default.
dbug: Microsoft.AspNetCore.Hosting.Internal.WebHost[4]
      Hosting started
dbug: Microsoft.AspNetCore.Hosting.Internal.WebHost[0]
      Loaded hosting startup assembly MvcCoreHybrid, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Running service 'MvcCoreHybrid'
Application Assembly: MvcCoreHybrid, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Hosting environment: Development
Content root path: /Users/jeremydmiller/code/jasper/src/MvcCoreHybrid
Hosted Service: Jasper.JasperActivator
Hosted Service: Jasper.Messaging.Logging.MetricsCollector
Hosted Service: Jasper.Messaging.BackPressureAgent
Listening for loopback messages

Active sending agent to tcp://localhost:2200/
Active sending agent to loopback://retries/

Application started. Press Ctrl+C to shut down.

```

If you stop that, and now see what other command line utilities Jasper has added to your project, type:

```
dotnet run -- help
```

And you'll see a lot of new options:

```
  -----------------------------------------------------------------------------------------------------------------------------------------------------------------
    Available commands:
  -----------------------------------------------------------------------------------------------------------------------------------------------------------------
        code -> Display or export the runtime, generated code in this application
    describe -> Preview the configuration of the Jasper application without running the application or starting Kestrel
         run -> Runs the configured Jasper application
    services -> Display the known Lamar service registrations
    validate -> Validate the configuration and environment for this Jasper application
  -----------------------------------------------------------------------------------------------------------------------------------------------------------------

```

See <[linkto:documentation/bootstrapping/console]> for more information about Jasper's command line support.





