using System;

namespace StorytellerSpecs.Fixtures.Marten.App
{
    public class TraceDoc
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
    }
}
