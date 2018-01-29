using System;
using BlueMilk.Codegen;
using BlueMilk.Codegen.Variables;
using BlueMilk.IoC.Frames;
using BlueMilk.IoC.Instances;
using BlueMilk.IoC.Resolvers;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMilk.IoC.Lazy
{
    public class FuncInstance<T> : Instance, IResolver
    {

        public FuncInstance() : base(typeof(Func<T>), typeof(Func<T>), ServiceLifetime.Transient)

        {
            Name = "func_of_" + typeof(T).NameInCode();
        }

        public override Variable CreateVariable(BuildMode mode, ResolverVariables variables, bool isRoot)
        {
            return new GetFuncFrame(this, typeof(T)).Variable;
        }
        

        public override bool RequiresServiceProvider { get; } = true;

        protected override IResolver buildResolver(Scope rootScope)
        {
            return this;
        }

        public object Resolve(Scope scope)
        {
            Func<T> func = scope.GetInstance<T>;

            return func;
        }

        public int Hash { get; set; }
        
    }
}