using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper.Bus;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Transports.Configuration;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Oakton;

namespace Jasper.CommandLine
{
    public enum SubscriptionsAction
    {
        list,
        export,
        publish,
        validate,
        remove
    }

    public class SubscriptionsInput : JasperInput
    {
        [Description("Choose the subscriptions action")]
        public SubscriptionsAction Action { get; set; } = SubscriptionsAction.list;

        [Description("Override the directory where subscription data is kept")]
        public string DirectoryFlag { get; set; } = Directory.GetCurrentDirectory();

        [Description("Override the file path to export or read the subscription data")]
        public string FileFlag { get; set; }

        [Description("Do not fail the execution if any errors are detected")]
        [FlagAlias("ignore-failures", 'i')]
        public bool IgnoreFailuresFlag { get; set; }
    }

    [Description("List or carry out administration on registered subscriptions")]
    public class SubscriptionsCommand : OaktonCommand<SubscriptionsInput>
    {
        public SubscriptionsCommand()
        {
            Usage("List the capabilities of this application");

            Usage("Administration of the subscriptions")
                .Arguments(x => x.Action);
        }

        public override bool Execute(SubscriptionsInput input)
        {
            if (input.Action == SubscriptionsAction.validate)
            {
                input.Registry.Settings.Alter<BusSettings>(x => x.ThrowOnValidationErrors = false);
            }

            using (var runtime = input.BuildRuntime())
            {
                switch (input.Action)
                {
                    case SubscriptionsAction.list:
                        writeList(runtime);
                        break;

                    case SubscriptionsAction.export:
                        export(runtime, input);
                        break;

                    case SubscriptionsAction.publish:
                        publish(runtime);
                        break;


                    case SubscriptionsAction.validate:
                        validate(runtime, input);
                        break;

                    case SubscriptionsAction.remove:
                        remove(runtime, input);
                        break;
                }
            }

            return true;
        }



        public async Task FanOutSubscriptionChangedMessage(IServiceBus bus, INodeDiscovery discovery)
        {
            var peers = await discovery.FindPeers();

            foreach (var node in peers)
            {
                var destination = node.DetermineLocalUri();
                if (destination != null)
                {
                    await bus.Send(destination, new SubscriptionsChanged());
                }
            }
        }

        private void validate(JasperRuntime runtime, SubscriptionsInput input)
        {
            var files = new FileSystem();
            var dict = new Dictionary<string, ServiceCapabilities>();

            files.FindFiles(input.DirectoryFlag, FileSet.Shallow("*.capabilities.json"))
                .Each(file =>
                {
                    try
                    {
                        Console.WriteLine("Reading " + file);

                        var capabilities = ServiceCapabilities.ReadFromFile(file);
                        if (dict.ContainsKey(capabilities.ServiceName))
                        {
                            ConsoleWriter.Write(ConsoleColor.Yellow,
                                $"Duplicate service name '{capabilities.ServiceName}' from file {file}");
                        }
                        else
                        {
                            dict.Add(capabilities.ServiceName, capabilities);
                        }
                    }
                    catch (Exception e)
                    {
                        ConsoleWriter.Write(ConsoleColor.Yellow, "Failed to read capabilities from file " + file);
                        Console.WriteLine(e);
                    }
                });

            if (dict.ContainsKey(runtime.ServiceName))
            {
                dict[runtime.ServiceName] = runtime.Capabilities;
            }
            else
            {
                dict.Add(runtime.ServiceName, runtime.Capabilities);
            }

            var messaging = new MessagingGraph(dict.Values.ToArray());

            Console.WriteLine(messaging.ToJson());

            if (input.FileFlag.IsNotEmpty())
            {
                Console.WriteLine("Writing the messaging graph to " + input.FileFlag);
                messaging.WriteToFile(input.FileFlag);
            }

            if (messaging.HasAnyErrors())
            {
                ConsoleWriter.Write(ConsoleColor.Yellow, "Messaging errors detected!");

                if (!input.IgnoreFailuresFlag)
                {
                    throw new Exception("Validation failures detected.");
                }
            }
            else
            {
                ConsoleWriter.Write(ConsoleColor.Green, "All messages have matching, valid publishers and subscribers");
            }
        }


        private void remove(JasperRuntime runtime, SubscriptionsInput input)
        {
            var repository = runtime.Get<ISubscriptionsRepository>();

            Console.WriteLine($"Removing all subscriptions for service {runtime.ServiceName} to {repository}");

            repository.ReplaceSubscriptions(runtime.ServiceName, new Subscription[0])
                .Wait(1.Minutes());

            sendSubscriptionUpdates(runtime);

            ConsoleWriter.Write(ConsoleColor.Green, "Success!");
        }

        private void publish(JasperRuntime runtime)
        {
            var repository = runtime.Get<ISubscriptionsRepository>();

            Console.WriteLine($"Writing subscriptions to {repository}");

            repository.ReplaceSubscriptions(runtime.ServiceName, runtime.Capabilities.Subscriptions)
                .Wait(1.Minutes());

            sendSubscriptionUpdates(runtime);

            ConsoleWriter.Write(ConsoleColor.Green, "Success!");
        }

        private static void sendSubscriptionUpdates(JasperRuntime runtime)
        {
            Console.WriteLine("Publishing a subscription-changed notification to all known nodes");
            var notifier = runtime.Get<SubscriptionChangeNotifier>();
            notifier.FanOutSubscriptionChangedMessage().Wait(1.Minutes());
        }

        private void export(JasperRuntime runtime, SubscriptionsInput input)
        {
            var file = input.FileFlag ?? input.DirectoryFlag
                           .AppendPath($"{runtime.ServiceName}.capabilities.json");

            Console.WriteLine("Writing subscriptions to file " + file);

            runtime.Capabilities.WriteToFile(file);

        }

        private void writeList(JasperRuntime runtime)
        {
            var json = runtime.Capabilities.ToJson();
            Console.WriteLine(json);
        }
    }

    public class SubscriptionChangeNotifier
    {
        private readonly IServiceBus _bus;
        private readonly INodeDiscovery _nodes;

        public SubscriptionChangeNotifier(IServiceBus bus, INodeDiscovery nodes)
        {
            _bus = bus;
            _nodes = nodes;
        }

        public async Task FanOutSubscriptionChangedMessage()
        {
            var peers = await _nodes.FindAllKnown()

            foreach (var node in peers)
            {
                var destination = node.DetermineLocalUri();
                if (destination != null)
                {
                    await _bus.Send(destination, new SubscriptionsChanged());
                }
            }
        }
    }
}
