using System;
using BlueMilk.Codegen;
using BlueMilk.Codegen.Variables;
using BlueMilk.IoC.Frames;
using BlueMilk.IoC.Instances;
using BlueMilk.IoC.Resolvers;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMilk.IoC.Lazy
{
    public class FuncInstance<T> : Instance
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

        public override object Resolve(Scope scope)
        {
            Func<T> func = scope.GetInstance<T>;

            return func;
        }

        
    }
}