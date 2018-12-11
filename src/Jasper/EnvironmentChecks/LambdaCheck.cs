using System;

namespace Jasper.EnvironmentChecks
{
    public class LambdaCheck : IEnvironmentCheck
    {
        private readonly Action<JasperRuntime> _action;

        public LambdaCheck(string description, Action<JasperRuntime> action)
        {
            Description = description;
            _action = action;
        }

        public void Assert(JasperRuntime runtime)
        {
            _action(runtime);
        }

        public string Description { get; }

        public override string ToString()
        {
            return Description;
        }
    }
}
