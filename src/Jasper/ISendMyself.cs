using System.Threading.Tasks;

namespace Jasper;

/// <summary>
/// Interface for cascading messages that require some customization of how
/// the resulting inner message is sent out
/// </summary>
public interface ISendMyself
{
    ValueTask ApplyAsync(IExecutionContext context);
}
