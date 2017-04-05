<!--title: Jasper Diagnostics-->

## Adding Diagnostics to your application

### ASP.NET Core App

To run diagnostics from an existing ASP.NET Core application, add the Diagnostic Services to your `JasperRegistry`

```csharp
public class BusRegistry : JasperBusRegistry
{
    public BusRegistry()
    {
        ...

        this.AddDiagnostics();
    }
}
```

Add the Diagnostics endpoint to your application configuration

```csharp
public void Configure(
  IApplicationBuilder app,
  IHostingEnvironment env,
  ILoggerFactory loggerFactory)
{
    app.UseDiagnostics();
}
```

### Standalone Application / Service Bus Application

To run diagnostics from a standalone application, such as a Jasper Bus application, use the `DiagnosticsFeature`.

```csharp
public class BusRegistry : JasperBusRegistry
{
    public BusRegistry()
    {
        Settings.Alter<DiagnosticsSettings>(_ =>
        {
            _.WebsocketPort = 3300;
        });

        Feature<DiagnosticsFeature>();
    }
}
```

## Viewing Diagnostics

By default the diagnostics endpoint can be found at `/_diag`.  You can change this in the `DiagnosticsSettings`.

```csharp
public class BusRegistry : JasperBusRegistry
{
    public BusRegistry()
    {
        Settings.Alter<DiagnosticsSettings>(_ =>
        {
            _.BasePath = "/_bus";
        });

        Feature<DiagnosticsFeature>();
    }
}
```

## Restricting access

You can restrict access to the diagnostics endpoint by using the `AuthorizeWith` property on the `DiagnosticsSettings`.

```csharp
app.UseDiagnostics(_ =>
{
    _.AuthorizeWith = context => context.User.HasClaim("admin", "true");
});
```
