using System;

namespace Jasper.Persistence.Testing.SqlServer.Durability.App
{
    public class TraceDoc
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
    }
}
