using System;

namespace Jasper.Internals.Codegen
{
    public interface IGenerates<T>
    {
        GeneratedClass ToClass(GenerationRules rules);

        string SourceCode { get; set; }

        T Create(Type[] types, Func<Type, object> container);

        string TypeName { get; }
    }
}
