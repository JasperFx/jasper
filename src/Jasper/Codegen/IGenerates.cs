using System.Reflection;
using StructureMap;

namespace Jasper.Codegen
{
    public interface IGenerates<T>
    {
        HandlerCode ToHandlerCode();

        string SourceCode { get; set; }

        T Create(Assembly assembly, IContainer container);
    }
}