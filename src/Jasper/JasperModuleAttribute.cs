using System;
using Baseline;
using Jasper.Configuration;

namespace Jasper
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class JasperModuleAttribute : Attribute
    {
        public Type ExtensionType { get; }

        public JasperModuleAttribute(Type extensionType)
        {
            ExtensionType = extensionType;
            if (!extensionType.CanBeCastTo<IJasperExtension>())
            {
                throw new ArgumentOutOfRangeException(nameof(extensionType), $"Has to be of type {nameof(IJasperExtension)}");
            }
        }
    }
}
