using System;
using BlueMilk.Codegen;

namespace BlueMilk.IoC
{
    public class BlueMilkMissingRegistrationException : BlueMilkException
    {
        public BlueMilkMissingRegistrationException(Type serviceType, string name) : base($"Unknown service registration '{name}' of {serviceType.FullNameInCode()}")
        {
        }

        public BlueMilkMissingRegistrationException(Type serviceType) : base($"No service registrations exist or can be derived for {serviceType.FullNameInCode()}")
        {
        }
    }
}