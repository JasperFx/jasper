using Jasper.Http.Model;

namespace Jasper.Http.ContentHandling
{
    public interface IReaderRule
    {
        bool TryToApply(RouteChain chain);
    }
}
