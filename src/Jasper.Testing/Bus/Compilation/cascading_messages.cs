using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Compilation
{
    public class source_code_attachment_to_handler_chains : IntegrationContext
    {
        [Fact]
        public void should_have_code_on_each_chain()
        {
            withAllDefaults();

            ShouldBeBooleanExtensions.ShouldBeTrue(Handlers.Chains.Any());

            foreach (var chain in Handlers.Chains)
            {
                ShouldBeStringTestExtensions.ShouldContain(chain.SourceCode, chain.TypeName);

//                Console.WriteLine("Should be " + chain.TypeName);
//                Console.WriteLine("--------------------------------------");
//                Console.WriteLine(chain.SourceCode);
//                Console.WriteLine("--------------------------------------");

            }
        }
    }



    public class cascading_messages : CompilationContext
    {
        public cascading_messages()
        {
            theRegistry.Handlers.IncludeType<CascadingHandler>();
        }

        [Fact]
        public async Task handle_a_single_cascading_message()
        {
            var input = new Incoming();
            var context = await Execute(input);

            context.OutgoingMessages().Single()
                .ShouldBeOfType<Output1>()
                .Id.ShouldBe(input.Id);
        }

        [Fact]
        public async Task handle_multiple_return_values()
        {
            var input = new Incoming2();

            var context = await Execute(input);

            var outgoing = context.OutgoingMessages().ToArray();

            outgoing.OfType<Output1>().Single().Id.ShouldBe(input.Id);
        }

        [Fact]
        public async Task can_capture_outgoing_from_cascading_messages()
        {
            var input = new Incoming3();

            var context = await Execute(input);

            context.OutgoingMessages().Count().ShouldBe(2);
        }
    }

    public class CascadingHandler
    {
        public Output1 Handle(Incoming input)
        {
            return new Output1{Id = input.Id};
        }

        public object[] Handle(Incoming2 input)
        {
            return new object[]
            {
                new Output2{Id = input.Id},
                new Output3{Id = input.Id},
                new Output4{Id = input.Id}
            };
        }

        public object Handle(IIncoming input)
        {
            return new Output1 {Id = input.Id};
        }

        public Task<Output1> Handle(Incoming3 input)
        {
            return Task.FromResult(new Output1 {Id = input.Id});
        }
    }

    public class Incoming
    {
        public Guid Id { get; set; }= Guid.NewGuid();
    }

    public interface IIncoming
    {
        Guid Id { get; }
    }

    public class Incoming2 : Incoming, IIncoming{}
    public class Incoming3 : Incoming, IIncoming{}

    public class Output1 : Incoming{}
    public class Output2 : Incoming{}
    public class Output3 : Incoming{}
    public class Output4 : Incoming{}






}
