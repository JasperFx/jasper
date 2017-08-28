using System;
using Oakton;

namespace Jasper.CommandLine
{
    public enum SubscriptionsAction
    {
        list,
        validate,
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
            throw new NotImplementedException();
        }
    }
}
