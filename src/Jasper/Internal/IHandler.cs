using System.Threading.Tasks;

namespace Jasper.Internal
{
    // What would partials look like though?
    public interface IHandler<TInput>
    {
        Task Handle(TInput input);
    }
}