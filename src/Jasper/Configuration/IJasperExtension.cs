using System;
using System.Reflection;
using System.Threading.Tasks;
using StructureMap;


namespace Jasper.Configuration
{
    // TODO -- Declare dependencies?
    public interface IJasperExtension
    {
        void Configure(JasperRegistry registry);
    }
}