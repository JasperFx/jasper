using Jasper.CommandLine;
using Jasper.Testing.Messaging.Lightweight;
using Xunit;

namespace Jasper.Testing.CommandLine
{
    public class CodeCommand_smoke_tester
    {
        //[Fact]
        public void can_generate_code()
        {
            var input = new CodeInput();
            input.Registry = new JasperRegistry();
            input.Registry.Handlers.DisableConventionalDiscovery();
            input.Registry.Handlers.IncludeType<MessageConsumer>();

            var cmd = new CodeCommand();
            cmd.Execute(input);
        }
    }
}
