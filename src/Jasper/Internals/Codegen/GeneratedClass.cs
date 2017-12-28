using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Baseline;
using Jasper.Internals.Compilation;

namespace Jasper.Internals.Codegen
{
    public class GeneratedClass
    {
        public GenerationRules Rules { get; }

        public string ClassName { get; }
        private Type _baseType;
        private readonly IList<Type> _interfaces = new List<Type>();
        private readonly IList<GeneratedMethod> _methods = new List<GeneratedMethod>();

        public GeneratedClass(GenerationRules rules, string className)
        {
            Rules = rules;
            ClassName = className;
        }

        public Visibility Visibility { get; set; } = Visibility.Public;

        public Type BaseType
        {
            get => _baseType;
            set
            {
                if (value == null)
                {
                    _baseType = null;
                    return;
                }

                if (value.GetTypeInfo().IsInterface)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Base type cannot be an interface");
                }

                _baseType = value;
            }
        }

        public void AddInterface(Type type)
        {
            if (!type.GetTypeInfo().IsInterface)
            {
                throw new ArgumentOutOfRangeException(nameof(type), "Must be an interface type");
            }

            _interfaces.Add(type);
        }

        public void AddInterface<T>()
        {
            AddInterface(typeof(T));
        }

        public IEnumerable<Type> Interfaces => _interfaces;


        public IEnumerable<GeneratedMethod> Methods => _methods;

        public void AddMethod(GeneratedMethod method)
        {
            method.ArrangeFrames(this);
            _methods.Add(method);
        }


        public void Write(ISourceWriter writer)
        {

            writeDeclaration(writer);

            var args = Args();
            writeFieldDeclarations(writer, args);
            writeConstructorMethod(writer, args);


            foreach (var method in _methods)
            {
                writer.BlankLine();
                method.WriteMethod(writer);
            }

            writer.FinishBlock();
        }

        public InjectedField[] Args()
        {
            var args = _methods.SelectMany(x => x.Fields).Distinct().ToArray();
            return args;
        }

        private void writeConstructorMethod(ISourceWriter writer, InjectedField[] args)
        {
            var ctorArgs = args.Select(x => x.CtorArgDeclaration).Join(", ");
            writer.Write($"BLOCK:public {ClassName}({ctorArgs})");

            foreach (var field in args)
            {
                field.WriteAssignment(writer);
            }

            writer.FinishBlock();
        }

        private void writeFieldDeclarations(ISourceWriter writer, InjectedField[] args)
        {
            foreach (var field in args)
            {
                field.WriteDeclaration(writer);
            }

            writer.BlankLine();
        }

        private void writeDeclaration(ISourceWriter writer)
        {
            var implemented = implements().ToArray();

            if (implemented.Any())
            {
                writer.Write($"BLOCK:public class {ClassName} : {implemented.Select(x => x.FullName).Join(", ")}");
            }
            else
            {
                writer.Write($"BLOCK:public class {ClassName}");
            }
        }

        private IEnumerable<Type> implements()
        {
            if (_baseType != null)
            {
                yield return _baseType;
            }

            foreach (var @interface in Interfaces)
            {
                yield return @interface;
            }
        }
    }

    public enum Visibility
    {
        Public,
        Protected,
        Private,
        Internal
    }


    public class Argument : Variable
    {
        public Argument(Type variableType, string usage) : base(variableType, usage)
        {
        }

        public string Declaration => $"{VariableType.FullName} {Usage}";

        public static Argument For<T>(string argName = null)
        {
            return new Argument(typeof(T), argName ?? DefaultArgName(typeof(T)));
        }

        protected bool Equals(Argument other)
        {
            return VariableType == other.VariableType && string.Equals(Usage, other.Usage);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Argument)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((VariableType != null ? VariableType.GetHashCode() : 0) * 397) ^ (Usage != null ? Usage.GetHashCode() : 0);
            }
        }
    }


}
