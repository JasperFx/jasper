using BlueMilk.Codegen;
using BlueMilk.Codegen.Variables;
using BlueMilk.IoC.Instances;

namespace BlueMilk.IoC.Frames
{
    public class InjectedServiceField : InjectedField, IServiceVariable
    {
        private bool _isOnlyOne;

        public InjectedServiceField(Instance instance) : base(instance.ServiceType,
            DefaultArgName(instance.ServiceType) + instance.GetHashCode().ToString().Replace("-", "_"))
        {
            Instance = instance;
        }

        public bool IsOnlyOne
        {
            private get => _isOnlyOne;
            set
            {
                _isOnlyOne = value;
                if (value)
                {
                    var defaultArgName = DefaultArgName(VariableType);
                    OverrideName("_" +defaultArgName);
                    CtorArg = defaultArgName;
                }
            }
        }

        public override string CtorArgDeclaration => IsOnlyOne
            ? $"{ArgType.NameInCode()} {CtorArg}"
            : $"[BlueMilk.Named(\"{Instance.Name}\")] {ArgType.NameInCode()} {CtorArg}";

        public Instance Instance { get; }
    }
}