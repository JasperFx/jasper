using Microsoft.Extensions.DependencyInjection;

namespace BlueMilk.Scanning.Conventions
{
    public interface IRegistrationConvention
    {
        void ScanTypes(TypeSet types, IServiceCollection services);
    }

}
