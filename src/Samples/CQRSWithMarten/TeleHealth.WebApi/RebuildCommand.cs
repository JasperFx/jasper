using Marten;
using Oakton;
using Spectre.Console;
using TeleHealth.Common;

namespace TeleHealth.WebApi;

[Description("Rebuild all projections")]
public class RebuildCommand : OaktonAsyncCommand<NetCoreInput>
{
    public override async Task<bool> Execute(NetCoreInput input)
    {
        using var host = input.BuildHost();
        var store = host.Services.GetRequiredService<IDocumentStore>();

        using var daemon = await store.BuildProjectionDaemonAsync();
        await daemon.StartDaemon();

        AnsiConsole.Write("[gray]Starting to rebuild Appointments[/]");
        await daemon.RebuildProjection<AppointmentProjection>(CancellationToken.None);
        AnsiConsole.Write("[green]Success![/]");
        
        AnsiConsole.Write("[gray]Starting to rebuild ProviderShift[/]");
        await daemon.RebuildProjection<ProviderShift>(CancellationToken.None);
        AnsiConsole.Write("[green]Success![/]");
        
        AnsiConsole.Write("[gray]Starting to rebuild BoardViews[/]");
        await daemon.RebuildProjection<BoardViewProjection>(CancellationToken.None);
        AnsiConsole.Write("[green]Success![/]");

        return true;
    }
}