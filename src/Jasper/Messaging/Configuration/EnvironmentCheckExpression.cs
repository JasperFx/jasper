using System;
using System.Threading.Tasks;
using Jasper.EnvironmentChecks;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Messaging.Configuration
{
    public class EnvironmentCheckExpression
    {
        private readonly JasperRegistry _parent;

        public EnvironmentCheckExpression(JasperRegistry parent)
        {
            _parent = parent;
        }

        /// <summary>
        /// Register a single environment check by type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Register<T>() where T : class, IEnvironmentCheck
        {
            _parent.Services.AddTransient<IEnvironmentCheck, T>();
        }

        /// <summary>
        /// Register a single environment check object
        /// </summary>
        /// <param name="check"></param>
        public void Register(IEnvironmentCheck check)
        {
            _parent.Services.AddSingleton<IEnvironmentCheck>(check);
        }

        public void Register(string description, Action<JasperRuntime> action)
        {
            Register(new LambdaCheck(description, action));
        }

        public void Register<T>(string description, Action<T> action)
        {
            Register(description, r => action(r.Get<T>()));
        }

        public void Register(string description, Action action)
        {
            Register(description, r => action());
        }

        public void Register<T>(string description, Func<T, Task> func, TimeSpan timeout)
        {
            Register(description, r =>
            {
                var task = func(r.Get<T>());
                task.Wait(timeout);

                if (!task.IsCompleted)
                {
                    throw new TimeoutException(description);
                }
            });
        }

        public void FileShouldExist(string file)
        {
            Register(new FileExistsCheck(file));
        }
    }
}
