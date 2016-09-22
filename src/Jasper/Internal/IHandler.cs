using System.Threading.Tasks;

namespace Jasper.Internal
{

    public interface IHandler<TInput>
    {
        Task Handle(TInput input);
    }
}