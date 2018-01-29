using System;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMilk.IoC.Resolvers
{
    public class SingletonLambdaResolver<T> : SingletonResolver<T> where T : class
    {
        private readonly Func<IServiceProvider, object> _builder;
        
        public SingletonLambdaResolver(Func<IServiceProvider, object> builder, Scope topLevelScope) : base(topLevelScope)
        {
            _builder = builder;
        }
        
        public override T Build(Scope scope)
        {
            return (T) _builder(scope.ServiceProvider);
        }
    }
}