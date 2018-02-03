using System;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMilk.IoC.Resolvers
{
    public class TransientLambdaResolver<T> : TransientResolver<T>
    {
        private readonly Func<IServiceProvider, object> _builder;
        
        public TransientLambdaResolver(Func<IServiceProvider, object> builder)
        {
            _builder = builder;
        }
        
        public override T Build(Scope scope)
        {
            // TODO -- have an overload that lets you use Func<IServiceProvider, T>
            return (T) _builder(scope.ServiceProvider);
        }
    }
}