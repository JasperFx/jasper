using System;

namespace Jasper
{
    public interface IExtensions
    {
        /// <summary>
        ///     Applies the extension to this application
        /// </summary>
        /// <param name="extension"></param>
        void Include(IJasperExtension extension);

        /// <summary>
        ///     Applies the extension with optional configuration to the application
        /// </summary>
        /// <param name="configure"></param>
        /// <typeparam name="T"></typeparam>
        void Include<T>(Action<T> configure = null) where T : IJasperExtension, new();

        T GetRegisteredExtension<T>();
    }
}
