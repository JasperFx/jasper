using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Baseline;
using LamarCodeGeneration.Frames;

namespace Jasper.Configuration
{
    public interface IModifyChain<T> where T : IChain
    {
        void Modify(T chain, JasperGenerationRules rules);
    }



    public abstract class Chain<TChain, TModifyAttribute> : IChain
        where TChain : Chain<TChain, TModifyAttribute>
        where TModifyAttribute : Attribute, IModifyChain<TChain>
    {
        public IList<Frame> Middleware { get; } = new List<Frame>();

        public IList<Frame> Postprocessors { get; } = new List<Frame>();
        public abstract string Description { get; }

        protected abstract MethodCall[] handlerCalls();

        private bool isConfigureMethod(MethodInfo method)
        {
            if (method.Name != "Configure") return false;

            if (method.GetParameters().Length != 1) return false;
            if (typeof(TChain).CanBeCastTo(method.GetParameters().Single().ParameterType)) return true;

            return false;
        }


        protected void applyAttributesAndConfigureMethods(JasperGenerationRules rules)
        {
            var handlers = handlerCalls();
            var configureMethods = handlers.Select(x => x.HandlerType).Distinct()
                .SelectMany(x => x.GetTypeInfo().GetMethods())
                .Where(isConfigureMethod);

            foreach (var method in configureMethods) method?.Invoke(null, new object[] {this});

            var handlerAtts = handlers.SelectMany(x => x.HandlerType.GetTypeInfo()
                .GetCustomAttributes<TModifyAttribute>());

            var methodAtts = handlers.SelectMany(x => x.Method.GetCustomAttributes<TModifyAttribute>());

            foreach (var attribute in handlerAtts.Concat(methodAtts)) attribute.Modify(this.As<TChain>(), rules);

            var genericHandlerAtts = handlers.SelectMany(x => x.HandlerType.GetTypeInfo()
                .GetCustomAttributes<ModifyChainAttribute>());

            var genericMethodAtts = handlers.SelectMany(x => x.Method.GetCustomAttributes<ModifyChainAttribute>());

            foreach (var attribute in genericHandlerAtts.Concat(genericMethodAtts)) attribute.Modify(this, rules);
        }
    }
}
