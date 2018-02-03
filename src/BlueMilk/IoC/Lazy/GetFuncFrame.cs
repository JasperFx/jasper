using System;
using System.Linq;
using Baseline;
using BlueMilk.Codegen;
using BlueMilk.Codegen.Frames;
using BlueMilk.Codegen.Variables;
using BlueMilk.IoC.Frames;
using BlueMilk.IoC.Instances;

namespace BlueMilk.IoC.Lazy
{
    public class GetFuncFrame : TemplateFrame
    {
        private object _scope;
        private readonly Type _serviceType;

        public GetFuncFrame(Instance instance, Type innerType)
        {
            _serviceType = innerType;
            Variable = new ServiceVariable(instance, this);
        }
        
        public Variable Variable { get; }

        protected override string Template()
        {
            _scope = Arg<Scope>();
            return $"System.Func<{_serviceType.FullNameInCode()}> {Variable.Usage} = () => {_scope}.{nameof(IContainer.GetInstance)}<{_serviceType.FullNameInCode()}>();";
        }
    }
}