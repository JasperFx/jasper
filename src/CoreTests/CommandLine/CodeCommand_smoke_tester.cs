using Jasper;
using Jasper.CommandLine;
using Jasper.Messaging.Configuration;
using Microsoft.AspNetCore.Hosting;
using Shouldly;
using TestingSupport;
using Xunit;

namespace CoreTests.CommandLine
{
    public class CodeCommand_smoke_tester
    {
        [Fact]
        public void can_generate_code()
        {
            var input = new CodeInput();
            var registry = new JasperRegistry();
            registry.Handlers.DisableConventionalDiscovery();
            registry.Handlers.IncludeType<MessageConsumer>();

            input.WebHostBuilder = JasperHost.CreateDefaultBuilder().UseJasper(registry);





            var cmd = new CodeCommand();
            cmd.Execute(input);
        }

        public enum SourceType
        {
            WebHostBuilder,
            JasperRegistry
        }

        [Theory]
        [InlineData(SourceType.JasperRegistry, new string[]{"code"})]
        [InlineData(SourceType.JasperRegistry, new string[]{"code", "all"})]
        [InlineData(SourceType.JasperRegistry, new string[]{"code", "messages"})]
        [InlineData(SourceType.JasperRegistry, new string[]{"code", "routes"})]
        [InlineData(SourceType.WebHostBuilder, new string[]{"code"})]
        [InlineData(SourceType.WebHostBuilder, new string[]{"code", "all"})]
        [InlineData(SourceType.WebHostBuilder, new string[]{"code", "messages"})]
        [InlineData(SourceType.WebHostBuilder, new string[]{"code", "routes"})]
        [InlineData(SourceType.JasperRegistry, new string[]{"services"})]
        [InlineData(SourceType.WebHostBuilder, new string[]{"services"})]



        [InlineData(SourceType.JasperRegistry, new string[]{"describe"})]
        [InlineData(SourceType.WebHostBuilder, new string[]{"describe"})]
        public void smoke_test_calls(SourceType sourceType, string[] args)
        {
            if (sourceType == SourceType.JasperRegistry)
            {
                var registry = new JasperRegistry(GetType().Assembly.GetName().Name);
                registry.Handlers.DisableConventionalDiscovery().IncludeType<MessageConsumer>();

                JasperHost.Run(registry, args).ShouldBe(0);
            }
            else
            {
                var builder = new WebHostBuilder();
                builder.UseStartup<EmptyStartup>().UseJasper(r =>
                    {
                        r.Handlers.DisableConventionalDiscovery().IncludeType<MessageConsumer>();
                    })
                    .RunJasper(args).ShouldBe(0);
            }
        }


    }

    public class Message1
    {
    }

    public class MessageConsumer
    {
        public void Handle(Message1 message)
        {
        }
    }

}
