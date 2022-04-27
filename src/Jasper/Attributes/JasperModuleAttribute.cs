using System;
using Baseline;
using Oakton;

namespace Jasper.Attributes
{
    /// <summary>
    ///     Marks the assembly as an automatically loaded Jasper extension
    ///     module
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class JasperModuleAttribute : OaktonCommandAssemblyAttribute
    {
        /// <summary>
        ///     Specify the IJasperExtension type that should be automatically loaded
        ///     and applied when this assembly is present
        /// </summary>
        /// <param name="jasperExtensionType"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public JasperModuleAttribute(Type jasperExtensionType)
        {
            JasperExtensionType = jasperExtensionType;
            if (!jasperExtensionType.CanBeCastTo<IJasperExtension>())
                throw new ArgumentOutOfRangeException(nameof(jasperExtensionType),
                    $"Has to be of type {nameof(IJasperExtension)}");
        }

        public JasperModuleAttribute()
        {
        }

        public Type? JasperExtensionType { get; }
    }
}
