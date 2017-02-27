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
        protected readonly GenerationConfig Config = new GenerationConfig("JasperCompilationTesting");

        private static int _number = 0;
        public readonly MethodChain theChain;
        public readonly ServiceRegistry services = new ServiceRegistry();
        private string _code = null;

        public CompilationContext()
        {
            theChain = new MethodChain($"Handler{++_number}",
                typeof(IInputHandler));

            _container = new Lazy<IContainer>(() => new Container(services));
        }

        protected string theGeneratedCode
        {
            get
            {
                if (_code == null)
                {
                    var set = buildHandlerSet();

                    _code = @set.GenerateCode();
                }

                return _code;
            }
        }

        private Lazy<IContainer> _container;

        private HandlerSet<MainInput, MethodChain> buildHandlerSet()
        {
            var config = new GenerationConfig("Jasper.Testing.Codegen.Generated");
            var container = _container.Value;
            config.Sources.Add(new StructureMapServices(container));

            config.Assemblies.Add(typeof(IContainer).GetTypeInfo().Assembly);
            config.Assemblies.Add(GetType().GetTypeInfo().Assembly);

            var @set = new HandlerSet<MainInput, MethodChain>(config, "input");

            @set.Add(theChain);

            return set;
        }

        private IInputHandler generate()
        {
            var @set = buildHandlerSet();

            var code = @set.GenerateCode();
            //Console.WriteLine(code);

            var type = @set.CompileAll().Single();

            return _container.Value.GetInstance(type).As<IInputHandler>();
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