using System.Threading;
using System.Threading.Tasks;
using Oakton.Resources;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Jasper.Persistence.Durability;

internal class EnvelopeStorageResource : IStatefulResource
{
    private readonly IEnvelopePersistence _persistence;

    public EnvelopeStorageResource(IEnvelopePersistence persistence)
    {
        _persistence = persistence;
    }

    public Task Check(CancellationToken token)
    {
        return _persistence.Admin.CheckAsync(token);
    }

    public Task ClearState(CancellationToken token)
    {
        return _persistence.Admin.ClearAllPersistedEnvelopes();
    }

    public Task Teardown(CancellationToken token)
    {
        return Task.CompletedTask;
    }

    public Task Setup(CancellationToken token)
    {
        return _persistence.Admin.RebuildStorage();
    }

    public async Task<IRenderable> DetermineStatus(CancellationToken token)
    {
        var counts = await _persistence.Admin.GetPersistedCounts();
        var table = new Table();
        table.AddColumns("Envelope Category", "Number");
        table.AddRow("Incoming", counts.Incoming.ToString());
        table.AddRow("Scheduled", counts.Scheduled.ToString());
        table.AddRow("Outgoing", counts.Outgoing.ToString());

        return table;
    }

    public string Type { get; } = "Jasper";
    public string Name { get; } = "Envelope Storage";
}
