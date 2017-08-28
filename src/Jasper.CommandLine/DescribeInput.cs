using Oakton;

namespace Jasper.CommandLine
{
    public class DescribeInput : JasperInput
    {

    }

    [Description("Just writes out details about the configured application")]
    public class DescribeCommand : OaktonCommand<DescribeInput>
    {
        public override bool Execute(DescribeInput input)
        {
            throw new System.NotImplementedException();
        }
    }
}
