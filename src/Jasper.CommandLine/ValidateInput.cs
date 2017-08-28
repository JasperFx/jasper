using Jasper.Bus;
using Oakton;

namespace Jasper.CommandLine
{
    public class ValidateInput : JasperInput
    {

    }

    [Description("Validate the configuration and environment for this Jasper application")]
    public class ValidateCommand : OaktonCommand<JasperInput>
    {
        public override bool Execute(JasperInput input)
        {
            input.Registry.Settings.Alter<BusSettings>(_ =>
            {
                _.ThrowOnValidationErrors = false;
            });

            using (var runtime = input.BuildRuntime())
            {
                // TODO -- actually validate here.
            }

            return true;
        }
    }

}
