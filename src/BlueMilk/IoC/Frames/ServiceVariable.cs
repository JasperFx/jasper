using System;
using BlueMilk.Codegen.Frames;
using BlueMilk.Codegen.Variables;
using BlueMilk.IoC.Instances;

namespace BlueMilk.IoC.Frames
{
    public enum ServiceDeclaration
    {
        ImplementationType,
        ServiceType
    }
    
    public class ServiceVariable : Variable, IServiceVariable
    {
        public ServiceVariable(Instance instance, Frame creator, ServiceDeclaration declaration = ServiceDeclaration.ImplementationType) 
            : base(declaration == ServiceDeclaration.ImplementationType ? instance.ImplementationType : instance.ServiceType, instance.Name.Replace(".", "_"), creator)
        {
            Instance = instance;
        }
        
        public Instance Instance { get; }
    }
}