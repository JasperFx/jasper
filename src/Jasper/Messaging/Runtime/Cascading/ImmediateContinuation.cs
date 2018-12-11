using System.Collections.Generic;
using System.Linq;
using Baseline;

namespace Jasper.Messaging.Runtime.Cascading
{
    public class ImmediateContinuation : IImmediateContinuation
    {
        private readonly IList<object> _actions = new List<object>();

        public ImmediateContinuation(params object[] actions)
        {
            _actions.AddRange(actions);
        }

        public object[] Actions()
        {
            return _actions.ToArray();
        }

        public ImmediateContinuation With(params object[] actions)
        {
            _actions.AddRange(actions);

            return this;
        }
    }
}
