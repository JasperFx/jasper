using System;
using Jasper;
using Jasper.Bus.Model;
using Jasper.CommandLine;
using Jasper.Marten;
using Oakton;

namespace ShowHandler
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var runtime = JasperRuntime.For(_ =>
            {
                _.MartenConnectionStringIs(Environment.GetEnvironmentVariable("marten_testing_database"));
            }))
            {
                var code = runtime.Get<HandlerGraph>().ChainFor<CreateItemCommand>()
                    .SourceCode;


                Console.WriteLine();
                Console.WriteLine();
                ConsoleWriter.Write(ConsoleColor.Cyan, "The source code for handling CreateItemCommand is:");
                Console.WriteLine(code);
            }
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
