using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Internals.Scanning.Conventions
{
    public interface IRegistrationConvention
    {
        void ScanTypes(TypeSet types, IServiceCollection services);
    }

}
