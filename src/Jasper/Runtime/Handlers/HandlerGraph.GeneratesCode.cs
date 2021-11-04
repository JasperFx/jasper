using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Baseline.ImTools;
using BaselineTypeDiscovery;
using Lamar;
using LamarCodeGeneration;
using LamarCodeGeneration.Model;

namespace Jasper.Runtime.Handlers
{
    public partial class HandlerGraph
    {
        IServiceVariableSource IGeneratesCode.AssemblyTypes(GenerationRules rules, GeneratedAssembly assembly)
        {
            foreach (var chain in Chains) chain.AssembleType(rules, assembly, Container);

            return Container.CreateServiceVariableSource();
        }

        async Task IGeneratesCode.AttachPreBuiltTypes(GenerationRules rules, Assembly assembly,
            IServiceProvider services)
        {
            var typeSet = await TypeRepository.ForAssembly(assembly);
            var handlerTypes = typeSet.ClosedTypes.Concretes.Where(x => x.CanBeCastTo<MessageHandler>()).ToArray();

            var container = (IContainer) services;

            foreach (var chain in Chains)
            {
                var handler = chain.AttachPreBuiltHandler(rules, container, handlerTypes);
                if (handler != null)
                {
                    _handlers = _handlers.Update(chain.MessageType, handler);
                }

            }
        }

        Task IGeneratesCode.AttachGeneratedTypes(GenerationRules rules, IServiceProvider services)
        {
            foreach (var chain in Chains)
            {
                var handler = chain.CreateHandler((IContainer) services);
                _handlers = _handlers.Update(chain.MessageType, handler);
            }

            return Task.CompletedTask;
        }

        string IGeneratesCode.CodeType => "Handlers";

    }
}
