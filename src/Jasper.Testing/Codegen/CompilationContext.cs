using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Jasper.Codegen;
using Jasper.Internal;

namespace Jasper.Testing.Codegen
{
    public class CompilationContext
    {
        private readonly GenerationConfig Config = new GenerationConfig("JasperCompilationTesting");

        private static int _number = 0;
        public readonly HandlerChain theChain;

        public CompilationContext()
        {
            theChain = new HandlerChain($"Handler{++_number}",
                typeof(IMainInputHandler));


        }

        private IHandler<MainInput> generate()
        {
            var config = new GenerationConfig("Jasper.Testing.Codegen.Generated");
            config.Assemblies.Add(GetType().GetTypeInfo().Assembly);

            var @set = new HandlerSet<MainInput, HandlerChain>(config, "input");

            @set.Add(theChain);

            var type = @set.CompileAll().Single();

            return (IHandler<MainInput>) Activator.CreateInstance(type);
        }

        protected Task<MainInput> afterRunning()
        {
            var handler = generate();
            var input = new MainInput();

            return handler.Handle(input).ContinueWith(t => input);
        }
    }

    public interface IMainInputHandler : IHandler<MainInput>
    {

    }

    public class MainInput
    {
        public bool WasTouched { get; private set; }

        public void Touch()
        {
            WasTouched = true;
        }

        public Task TouchAsync()
        {
            return Task.Factory.StartNew(Touch);
        }
    }
}