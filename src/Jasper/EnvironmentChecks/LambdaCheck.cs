using System;

namespace Jasper.EnvironmentChecks
{
    public class LambdaCheck : IEnvironmentCheck
    {
        private readonly string _description;
        private readonly Action<JasperRuntime> _action;

        public LambdaCheck(string description, Action<JasperRuntime> action)
        {
            _description = description;
            _action = action;
        }

        public void Assert(JasperRuntime runtime)
        {
            _action(runtime);
        }

        public override string ToString()
        {
            return _description;
        }
    }
}