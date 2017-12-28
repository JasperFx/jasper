using Microsoft.Extensions.DependencyInjection;

namespace BlueMilk.IoC
{
    public interface IServiceDescriptorBuildStep
    {
        ServiceDescriptor ServiceDescriptor { get; }
        bool CanBeReused { get; }
    }
}
