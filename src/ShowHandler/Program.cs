

using System;
using Jasper;
using Jasper.CommandLine;
using Jasper.Marten;
using Jasper.Messaging.Model;
using Oakton;

// SAMPLE: using-JasperModule-with-no-extension
[assembly:JasperModule]
// ENDSAMPLE

namespace ShowHandler
{
    class Program
    {
        static int Main(string[] args)
        {
            return JasperAgent.Run(args, _ =>
            {
                _.MartenConnectionStringIs("Host=localhost;Port=5433;Database=postgres;Username=postgres;password=postgres");
                _.Include<MartenBackedPersistence>();
            });
        }
    }




    public class CreateItemCommand
    {
        public string Name { get; set; }
    }

    public class ItemCreatedEvent
    {
        public Item Item { get; set; }
    }

    public class Item
    {
        public Guid Id;
        public string Name;
    }
}
