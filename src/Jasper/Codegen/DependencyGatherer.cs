using System.Collections.Generic;
using System.Linq;
using Baseline;

namespace Jasper.Codegen
{
    public class DependencyGatherer
    {
        private readonly IList<Frame> _frames;
        private readonly IList<Variable> _variables = new List<Variable>();

        public readonly LightweightCache<Frame, List<Frame>> Dependencies = new LightweightCache<Frame, List<Frame>>();

        public DependencyGatherer(IList<Frame> frames)
        {
            _frames = frames;

            Dependencies.OnMissing = frame => new List<Frame>(findDependencies(frame).Distinct());

            foreach (var frame in frames)
            {
                Dependencies.FillDefault(frame);
            }
        }

        private IEnumerable<Frame> findDependencies(Frame frame)
        {
            foreach (var dependency in frame.Dependencies)
            {
                yield return dependency;

                foreach (var child in Dependencies[dependency])
                {
                    yield return child;
                }
            }

            foreach (var variable in frame.Uses)
            {
                if (variable.Creator == null) continue;

                yield return variable.Creator;

                foreach (var child in Dependencies[variable.Creator])
                {
                    yield return child;
                }
            }
        }

    }
}