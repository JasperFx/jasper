using Jasper.Bus.Model;

namespace Jasper.Bus.Configuration
{
    public class MaximumAttemptsAttribute : ModifyHandlerChainAttribute
    {
        private readonly int _attempts;

        public MaximumAttemptsAttribute(int attempts)
        {
            _attempts = attempts;
        }

        public override void Modify(HandlerChain chain)
        {
            chain.MaximumAttempts = _attempts;
        }
    }
}
