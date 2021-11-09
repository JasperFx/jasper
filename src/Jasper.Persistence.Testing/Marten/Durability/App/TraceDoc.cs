using System;

namespace Jasper.Persistence.Testing.Marten.Durability.App
{
    public class TraceDoc
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
    }
}
