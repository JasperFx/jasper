using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using BlueMilk.Codegen.Frames;
using BlueMilk.Codegen.ServiceLocation;
using BlueMilk.Codegen.Variables;
using BlueMilk.Compilation;
using BlueMilk.IoC;
using BlueMilk.IoC.Frames;
using BlueMilk.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMilk.Codegen
{
    public class ServiceVariableSource : IVariableSource
    {
        private readonly ServiceGraph _services;
        private readonly IList<ServiceStandinVariable> _standins = new List<ServiceStandinVariable>();

        public ServiceVariableSource(ServiceGraph services)
        {
            _services = services;
        }

        public bool Matches(Type type)
        {
            return _services.CouldResolve(type);
        }

        public Variable Create(Type type)
        {
            var instance = _services.FindDefault(type);
            var standin =  new ServiceStandinVariable(instance);
            _standins.Add(standin);

            return standin;
        }
        
        // TODO -- later, do we use other variables?
        public void ReplaceVariables()
        {
            if (_standins.Any(x => x.Instance.RequiresServiceProvider))
            {
                useServiceProvider();
            }
            else
            {
                useInlineConstruction();
            }
        }

        private void useInlineConstruction()
        {
            var variables = new ResolverVariables();
            foreach (var standin in _standins)
            {
                var variable = variables.Resolve(standin.Instance, BuildMode.Inline);
                standin.UseInner(variable);
            }

            variables.OfType<InjectedServiceField>().Each(field =>
            {
                var family = _services.FindAll(field.VariableType);
                field.IsOnlyOne = family.Length == 1;
            });
        }

        private void useServiceProvider()
        {
            var factory = new InjectedField(typeof(IServiceScopeFactory));
            var createScope = new ServiceScopeFactoryCreation(factory);
            var provider = createScope.Provider;

            foreach (var standin in _standins)
            {
                var variable = new GetServiceFrame(provider, standin.VariableType).Variable;
                standin.UseInner(variable);
            }
        }
    }
    
    public class ServiceStandinVariable : Variable
    {
        public Instance Instance { get; }

        public ServiceStandinVariable(Instance instance) : base(instance.ServiceType)
        {
            Instance = instance;
        }

        public void UseInner(Variable variable)
        {
            OverrideName(variable.Usage);
            Dependencies.Add(variable);
        }
    }

    public class GetServiceFrame : SyncFrame
    {
        private readonly Variable _provider;

        public GetServiceFrame(Variable provider, Type serviceType)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            uses.Add(provider);
            
            Variable = new Variable(serviceType, this);
        }
        
        public Variable Variable { get; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"var {Variable.Usage} = ({Variable.VariableType.FullNameInCode()}){_provider.Usage}.{nameof(IServiceProvider.GetService)}(typeof({Variable.VariableType.FullNameInCode()}));");
            Next?.GenerateCode(method, writer);
        }
    }
}