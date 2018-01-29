using BlueMilk.Codegen;
using BlueMilk.Codegen.Variables;
using BlueMilk.IoC.Frames;
using BlueMilk.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMilk.IoC.Resolvers
{
    public class ScopeInstance<T> : Instance, IResolver
    {
        public ScopeInstance() : base(typeof(T), typeof(T), ServiceLifetime.Scoped)
        {
            
            Name = typeof(T).Name;
        }

        public override Variable CreateVariable(BuildMode mode, ResolverVariables variables, bool isRoot)
        {
            return new CastScopeFrame(typeof(T)).Variable;
        }

        protected override IResolver buildResolver(Scope rootScope)
        {
            return this;
        }

        public object Resolve(Scope scope)
        {
            return scope;
        }

        public int Hash { get; set; }

        public override string ToString()
        {
            return $"Current {typeof(T).NameInCode()}";
        }
    }
}