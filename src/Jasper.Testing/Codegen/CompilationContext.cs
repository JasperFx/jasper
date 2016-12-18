using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Jasper.Codegen;
using Jasper.Codegen.StructureMap;
using Jasper.Configuration;
using Jasper.Internal;
using StructureMap;

namespace Jasper.Testing.Codegen
{
    public class CompilationContext
    {
        private readonly GenerationConfig Config = new GenerationConfig("JasperCompilationTesting");

        private static int _number = 0;
        public readonly HandlerChain theChain;
        public readonly ServiceRegistry services = new ServiceRegistry();

        public CompilationContext()
        {
            theChain = new HandlerChain($"Handler{++_number}",
                typeof(IInputHandler));


        }

        protected string theGeneratedCode
        {
            get
            {
                var config = new GenerationConfig("Jasper.Testing.Codegen.Generated");
                var container = new Container(services);
                config.Sources.Add(new StructureMapServices(container));

                config.Assemblies.Add(GetType().GetTypeInfo().Assembly);

                var @set = new HandlerSet<MainInput, HandlerChain>(config, "input");

                @set.Add(theChain);

                return @set.GenerateCode();
            }
        }

        private IInputHandler generate()
        {
            var config = new GenerationConfig("Jasper.Testing.Codegen.Generated");

            var container = new Container(services);
            config.Sources.Add(new StructureMapServices(container));

            config.Assemblies.Add(GetType().GetTypeInfo().Assembly);

            var @set = new HandlerSet<MainInput, HandlerChain>(config, "input");

            @set.Add(theChain);

            var code = @set.GenerateCode();
            Console.WriteLine(code);

            var type = @set.CompileAll().Single();

            return container.GetInstance(type).As<IInputHandler>();
        }

        protected Task<MainInput> afterRunning()
        {
            var handler = generate();
            var input = new MainInput();

            return handler.Handle(input).ContinueWith(t => input);
        }


    }

    public interface IInputHandler : IHandler<MainInput>
    {

    }

    public class MainInput
    {
        public bool WasTouched { get; private set; }

        public void Touch()
        {
            WasTouched = true;
        }

        public void DoSync()
        {

        }

        public Task TouchAsync()
        {
            return Task.Factory.StartNew(Touch);
        }

        public Task DifferentAsync()
        {
            DifferentWasCalled = true;
            return Task.CompletedTask;
        }

        public bool DifferentWasCalled { get; set; }
    }

}