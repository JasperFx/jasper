using System;

namespace DurabilitySpecs.Fixtures.SqlServer.App
{
    public class TraceDoc
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
    }
}
