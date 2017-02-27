using System;
using System.Collections.Generic;
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
    public class SimpleChain : IGenerates<IInputHandler>
    {
        private static int _number = 0;

        public readonly HandlerCode HandlerCode = new HandlerCode($"Handler{++_number}",
            typeof(IInputHandler));

        public HandlerCode ToHandlerCode()
        {
            return HandlerCode;
        }

        public string SourceCode { get; set; }

        public IInputHandler Create(Type[] types, IContainer container)
        {
            var type = types.FirstOrDefault(x => x.Name == HandlerCode.ClassName);
            return container.GetInstance(type).As<IInputHandler>();
        }

        public string TypeName => HandlerCode.ClassName;
    }

    public class SimpleHandlerSet : HandlerSet<SimpleChain, MainInput, IInputHandler>
    {
        public SimpleHandlerSet(GenerationConfig config, string inputArgName) : base(config, inputArgName)
        {
        }

        private readonly IList<SimpleChain> _chains = new List<SimpleChain>();
        public void Add(SimpleChain chain)
        {
            _chains.Add(chain);
        }

        protected override SimpleChain[] chains => _chains.ToArray();
    }


    public class CompilationContext
    {
        protected readonly GenerationConfig Config = new GenerationConfig("JasperCompilationTesting");

        private static int _number = 0;
        public readonly SimpleChain _parent;

        public readonly ServiceRegistry services = new ServiceRegistry();
        private string _code = null;

        public CompilationContext()
        {
            _parent = new SimpleChain();

            _container = new Lazy<IContainer>(() => new Container(services));
        }

        protected HandlerCode theChain => _parent.HandlerCode;

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

        private readonly Lazy<IContainer> _container;

        private HandlerSet<SimpleChain, MainInput, IInputHandler> buildHandlerSet()
        {
            var config = new GenerationConfig("Jasper.Testing.Codegen.Generated");
            var container = _container.Value;
            config.Sources.Add(new StructureMapServices(container));

            config.Assemblies.Add(typeof(IContainer).GetTypeInfo().Assembly);
            config.Assemblies.Add(GetType().GetTypeInfo().Assembly);

            var @set = new SimpleHandlerSet(config, "input");

            @set.Add(_parent);

            return set;
        }

        private IInputHandler generate()
        {
            var @set = buildHandlerSet();

            return @set.CompileAndBuildAll(_container.Value).Single();
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