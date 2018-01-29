using System;
using System.Linq;
using Baseline;
using BlueMilk.Codegen;
using BlueMilk.Codegen.Frames;

namespace Jasper.Configuration
{
    /// <summary>
    /// Attach one or more Jasper middleware frames by type
    /// </summary>
    public class MiddlewareAttribute : ModifyChainAttribute
    {
        private readonly Type[] _frameTypes;

        public MiddlewareAttribute(params Type[] frameTypes)
        {
            var notMatching = frameTypes.Where(x => !x.IsConcreteWithDefaultCtor() || !x.CanBeCastTo<Frame>()).ToArray();
            if (notMatching.Any())
            {
                throw new ArgumentOutOfRangeException($"Invalid Frame types: {notMatching.Select(x => x.FullName).Join(", ")}");
            }

            _frameTypes = frameTypes;
        }

        public override void Modify(IChain chain)
        {
            chain.Middleware.AddRange(_frameTypes.Select(x => Activator.CreateInstance(x).As<Frame>()));
        }
    }
}
