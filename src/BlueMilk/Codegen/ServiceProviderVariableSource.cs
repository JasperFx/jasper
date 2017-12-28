using System;
using BlueMilk.Codegen.ServiceLocation;

namespace BlueMilk.Codegen
{
    public class ServiceProviderVariableSource : IVariableSource
    {
        public static readonly ServiceProviderVariableSource Instance = new ServiceProviderVariableSource();

        private ServiceProviderVariableSource()
        {
        }

        public bool Matches(Type type)
        {
            return type == typeof(IServiceProvider);
        }

        public Variable Create(Type type)
        {
            return new ServiceScopeFactoryCreation().Provider;
        }
    }
}