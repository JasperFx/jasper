using System;
using StructureMap;

namespace Jasper.Codegen
{
    public interface IGenerates<T>
    {
        IHandlerGeneration ToHandlerCode(GenerationConfig config);

        string SourceCode { get; set; }

        T Create(Type[] types, IContainer container);

        string TypeName { get; }
    }
}