using System;

namespace StorytellerSpecs.Fixtures.SqlServer.App
{
    public class TraceDoc
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
    }
}
