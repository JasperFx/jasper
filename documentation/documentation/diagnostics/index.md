<!--title: Jasper Diagnostics-->

## Adding Diagnostics to your application

Add the Diagnostic Services to your `JasperRegistry`

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

## Restricting access

You can restrict access to the diagnostics endpoint by using the `AuthorizeWith` property on the `DiagnosticsSettings`.

```csharp
app.UseDiagnostics(_ =>
{
    _.AuthorizeWith = context => context.User.HasClaim("admin", "true");
});
```
