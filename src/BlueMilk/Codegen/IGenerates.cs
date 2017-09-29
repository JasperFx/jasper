using System;

namespace BlueMilk.Codegen
{
    public interface IGenerates<T>
    {
        GeneratedClass ToClass(GenerationRules rules);

        string SourceCode { get; set; }

        T Create(Type[] types, Func<Type, object> container);

        string TypeName { get; }
    }
}
