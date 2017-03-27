using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Microsoft.Extensions.Configuration;
using StructureMap.Graph;
using StructureMap.Pipeline;

namespace Jasper.Settings
{
    public class SettingsPolicy : IFamilyPolicy
    {
        public bool AppliesToHasFamilyChecks => true;

        public PluginFamily Build(Type type)
        {
            if (type.Name.EndsWith("Settings") && type.IsConcreteWithDefaultCtor())
            {
                var family = new PluginFamily(type);
                var instance = buildInstanceForType(type);
                family.SetDefault(instance);

                return family;
            }

            return null;
        }

        private static Instance buildInstanceForType(Type type)
        {
            var instanceType = typeof(SettingsInstance<>).MakeGenericType(type);
            var instance = Activator.CreateInstance(instanceType).As<Instance>();
            return instance;
        }
    }

    public class SettingsInstance<T> : LambdaInstance<T> where T : class, new()
    {
        public SettingsInstance() : base(
            $"Building {typeof(T).FullName} from application settings",
            c => c.GetInstance<IConfigurationRoot>().Get<T>())
        {
            LifecycleIs<SingletonLifecycle>();
        }
    }
}
