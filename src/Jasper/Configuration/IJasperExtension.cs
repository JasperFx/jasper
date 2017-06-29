using System;
using System.Reflection;
using System.Threading.Tasks;
using StructureMap;


namespace Jasper.Configuration
{
    public interface IJasperExtension
    {
        void Configure(JasperRegistry registry);
    }
}
