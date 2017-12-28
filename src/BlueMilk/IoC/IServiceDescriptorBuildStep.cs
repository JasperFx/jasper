using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Internals.IoC
{
    public interface IServiceDescriptorBuildStep
    {
        ServiceDescriptor ServiceDescriptor { get; }
        bool CanBeReused { get; }
    }
}
