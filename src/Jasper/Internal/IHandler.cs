using System;
using System.Threading.Tasks;

namespace Jasper.Internal
{
    [Obsolete("Want to get rid of this one")]
    public interface IHandler<TInput>
    {
        Task Handle(TInput input);
    }
}
