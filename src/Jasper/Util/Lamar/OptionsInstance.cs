using System;
using Lamar.IoC;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;
using LamarCompiler.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Jasper.Util.Lamar
{
    internal class OptionsInstance<T> : Instance where T : class, new()
    {
        public OptionsInstance() : base(typeof(IOptions<T>), typeof(OptionsManager<T>), ServiceLifetime.Singleton)
        {
        }
        
        public override Func<Scope, object> ToResolver(Scope topScope)
        {
            return s => resolveFromRoot(topScope);
        }

        public override object Resolve(Scope scope)
        {
            return resolveFromRoot(scope.Root);
        }
        
        private readonly object _locker = new object();

        private object resolveFromRoot(Scope root)
        {
            if (tryGetService(root, out object service))
            {
                return service;
            }

            lock (_locker)
            {
                if (tryGetService(root, out service))
                {
                    return service;
                }
                
                var setups = root.QuickBuildAll<IConfigureOptions<T>>();
                var postConfigures = root.QuickBuildAll<IPostConfigureOptions<T>>();

                var options = new OptionsManager<T>(new OptionsFactory<T>(setups, postConfigures));
            
            
                store(root, options);

                return options;
            }


        }

        public override Variable CreateVariable(BuildMode mode, ResolverVariables variables, bool isRoot)
        {
            return new InjectedServiceField(this);
        }
    }
}