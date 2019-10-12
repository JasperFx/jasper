using JasperHttp.Model;

namespace JasperHttp.ContentHandling
{
    public interface IReaderRule
    {
        bool TryToApply(RouteChain chain);
    }
}
