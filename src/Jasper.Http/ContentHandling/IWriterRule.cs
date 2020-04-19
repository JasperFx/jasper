using Jasper.Http.Model;

namespace Jasper.Http.ContentHandling
{
    public interface IWriterRule
    {
        bool TryToApply(RouteChain chain);
    }
}
