using System;

namespace BlueMilk.Codegen
{
    public interface IGenerates<T>
    {
        GeneratedClass ToClass(GenerationConfig config);

        string SourceCode { get; set; }

        T Create(Type[] types, Func<Type, object> container);

        string TypeName { get; }
    }
}
