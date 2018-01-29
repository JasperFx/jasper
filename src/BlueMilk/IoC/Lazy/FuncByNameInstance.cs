using System;
using BlueMilk.Codegen;
using BlueMilk.Codegen.Variables;
using BlueMilk.IoC.Frames;
using BlueMilk.IoC.Instances;
using BlueMilk.IoC.Resolvers;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMilk.IoC.Lazy
{
    public class FuncByNameInstance<T> : Instance, IResolver
    {

        public FuncByNameInstance() : base(typeof(Func<string, T>), typeof(Func<string, T>), ServiceLifetime.Transient)
        {
            Name = "func_by_name_of_" + typeof(T).NameInCode();
        }

        public override Variable CreateVariable(BuildMode mode, ResolverVariables variables, bool isRoot)
        {
            return new GetFuncByNameFrame(this, typeof(T)).Variable;
        }
        

        public override bool RequiresServiceProvider { get; } = true;

        protected override IResolver buildResolver(Scope rootScope)
        {
            return this;
        }

        public object Resolve(Scope scope)
        {
            T Func(string name) => scope.GetInstance<T>(name);

            return (Func<string, T>) Func;
        }

        public int Hash { get; set; }
        
    }
}