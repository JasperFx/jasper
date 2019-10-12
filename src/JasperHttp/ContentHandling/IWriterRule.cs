using JasperHttp.Model;

namespace JasperHttp.ContentHandling
{

    public interface IWriterRule
    {
        bool TryToApply(RouteChain chain);
    }
}
