using System;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMilk.IoC.Resolvers
{
    public class ScopedLambdaResolver<T> : ScopedResolver<T>
    {
        private readonly Func<IServiceProvider, object> _builder;

        public ScopedLambdaResolver(Func<IServiceProvider, object> builder)
        {
            _builder = builder;
        }

        public override T Build(Scope scope)
        {
            // TODO -- have an overload where you use Func<IServiceProvider, T>
            return (T) _builder(scope.ServiceProvider);
        }
    }
}