using System;
using Baseline;
using Jasper.Configuration;

namespace Jasper
{
    /// <summary>
    /// Marks the assembly as an automatically loaded Jasper extension
    /// module
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class JasperModuleAttribute : Attribute
    {
        public Type ExtensionType { get; }

        /// <summary>
        /// Specify the IJasperExtension type that should be automatically loaded
        /// and applied when this assembly is present
        /// </summary>
        /// <param name="extensionType"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public JasperModuleAttribute(Type extensionType)
        {
            ExtensionType = extensionType;
            if (!extensionType.CanBeCastTo<IJasperExtension>())
            {
                throw new ArgumentOutOfRangeException(nameof(extensionType), $"Has to be of type {nameof(IJasperExtension)}");
            }
        }

        public JasperModuleAttribute()
        {
        }
    }
}
