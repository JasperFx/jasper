using System;
using System.Threading.Tasks;
using Jasper.EnvironmentChecks;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Messaging.Configuration
{
    public static class EnvironmentCheckServiceExtensions
    {
        /// <summary>
        /// Shorthand to register an IEnvironmentCheck service
        /// </summary>
        /// <param name="services"></param>
        /// <typeparam name="T"></typeparam>
        public static void EnvironmentCheck(this IServiceCollection services, IEnvironmentCheck check)
        {
            services.AddSingleton<IEnvironmentCheck>(check);
        }

        /// <summary>
        /// Shorthand to register an IEnvironmentCheck service
        /// </summary>
        /// <param name="services"></param>
        /// <typeparam name="T"></typeparam>
        public static void EnvironmentCheck<T>(this IServiceCollection services) where T : class, IEnvironmentCheck
        {
            services.AddTransient<IEnvironmentCheck, T>();
        }

        /// <summary>
        /// Register an environment check for the supplied action
        /// </summary>
        /// <param name="services"></param>
        /// <param name="description"></param>
        /// <param name="action"></param>
        public static void EnvironmentCheck(this IServiceCollection services, string description, Action<JasperRuntime> action)
        {
            services.AddSingleton<IEnvironmentCheck>(new LambdaCheck(description, action));
        }

        /// <summary>
        /// Register an environment check for the supplied action using a registered service T
        /// </summary>
        /// <param name="services"></param>
        /// <param name="description"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        public static void EnvironmentCheck<T>(this IServiceCollection services, string description, Action<T> action)
        {
            services.EnvironmentCheck(description, r => action(r.Get<T>()));
        }

        /// <summary>
        /// Register an environment check for the supplied action
        /// </summary>
        /// <param name="services"></param>
        /// <param name="description"></param>
        /// <param name="action"></param>
        public static void EnvironmentCheck(this IServiceCollection services, string description, Action action)
        {
            services.EnvironmentCheck(description, r => action());
        }

        /// <summary>
        /// Register an environment check for an asynchronous operation
        /// </summary>
        /// <param name="services"></param>
        /// <param name="description"></param>
        /// <param name="func"></param>
        /// <param name="timeout"></param>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="TimeoutException"></exception>
        public static void EnvironmentCheck<T>(this IServiceCollection services, string description, Func<T, Task> func, TimeSpan timeout)
        {
            services.EnvironmentCheck(description, r =>
            {
                var task = func(r.Get<T>());
                task.Wait(timeout);

                if (!task.IsCompleted)
                {
                    throw new TimeoutException(description);
                }
            });
        }

        /// <summary>
        /// Register an environment check for the existence of the named file
        /// </summary>
        /// <param name="services"></param>
        /// <param name="file"></param>
        public static void CheckFileExists(this IServiceCollection services, string file)
        {
            services.AddSingleton<IEnvironmentCheck>(new FileExistsCheck(file));
        }
    }
}
