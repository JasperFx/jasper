using System;
using Jasper.Bus;
using Oakton;

namespace Jasper.CommandLine
{
    public enum SubscriptionsAction
    {
        list,
        export,
        publish,
        delta
    }

    public class SubscriptionsInput : JasperInput
    {
        [Description("Choose the subscriptions action")]
        public SubscriptionsAction Action { get; set; } = SubscriptionsAction.list;

        [Description("Override the directory where subscription data is kept")]
        public string DirectoryFlag { get; set; }

        [Description("Override the file path to export or read the subscription data")]
        public string FileFlag { get; set; }
    }

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
                }
            }

            return true;
        }


        private void publish(JasperRuntime runtime)
        {
            throw new NotImplementedException();
        }

        private void export(JasperRuntime runtime, SubscriptionsInput input)
        {
            throw new NotImplementedException();
        }

        private void writeList(JasperRuntime runtime)
        {
            throw new NotImplementedException();
        }
    }
}
