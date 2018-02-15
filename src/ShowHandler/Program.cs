

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
                _.MartenConnectionStringIs(Environment.GetEnvironmentVariable("marten_testing_database"));
            });
        }
    }


    [Description("Show the code for the CreateItemCommand handler", Name = "show")]
    public class ShowCodeCommand : OaktonCommand<JasperInput>
    {
        public override bool Execute(JasperInput input)
        {
            using (var runtime = input.BuildRuntime())
            {
                var code = runtime.Get<HandlerGraph>().ChainFor<CreateItemCommand>()
                    .SourceCode;


                Console.WriteLine();
                Console.WriteLine();
                ConsoleWriter.Write(ConsoleColor.Cyan, "The source code for handling CreateItemCommand is:");
                Console.WriteLine(code);
            }

            return true;
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
