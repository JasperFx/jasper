using System;
using StructureMap;

namespace Jasper.Codegen
{
    public interface IGenerates<T>
    {
        GeneratedClass ToClass(IGenerationConfig config);

        [Obsolete("going to get rid of this one")]
        IGenerationModel ToGenerationModel(IGenerationConfig config);

        string SourceCode { get; set; }

        T Create(Type[] types, IContainer container);

        string TypeName { get; }
    }
}
