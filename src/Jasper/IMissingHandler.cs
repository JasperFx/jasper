using System.Threading.Tasks;
using Jasper.Runtime;

namespace Jasper;

#region sample_IMissingHandler

/// <summary>
///     Hook interface to receive notifications of envelopes received
///     that do not match any known handlers within the system
/// </summary>
public interface IMissingHandler
{
    /// <summary>
    ///     Executes for unhandled envelopes
    /// </summary>
    /// <param name="envelope"></param>
    /// <param name="root"></param>
    /// <returns></returns>
    ValueTask HandleAsync(Envelope? envelope, IJasperRuntime root);
}

#endregion
