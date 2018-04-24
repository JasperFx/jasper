using System;
using System.Collections.Generic;
using Jasper.Configuration;
using Lamar;

namespace Jasper
{
    public partial class JasperRegistry
    {
        private readonly List<IJasperExtension> _appliedExtensions = new List<IJasperExtension>();

        internal ServiceRegistry ExtensionServices { get; } = new ExtensionServiceRegistry();

        internal void ApplyExtensions(IJasperExtension[] extensions)
        {
            Settings.ApplyingExtensions = true;
            Services = ExtensionServices;


            foreach (var extension in extensions)
            {
                extension.Configure(this);
                _appliedExtensions.Add(extension);
            }



            Services = _applicationServices;
            Settings.ApplyingExtensions = false;
        }

        /// <summary>
        /// Read only view of the extensions that have been applied to this
        /// JasperRegistry
        /// </summary>
        public IReadOnlyList<IJasperExtension> AppliedExtensions => _appliedExtensions;

        /// <summary>
        ///     Applies the extension to this application
        /// </summary>
        /// <param name="extension"></param>
        public void Include(IJasperExtension extension)
        {
            ApplyExtensions(new[] {extension});
        }

        /// <summary>
        ///     Applies the extension with optional configuration to the application
        /// </summary>
        /// <param name="configure"></param>
        /// <typeparam name="T"></typeparam>
        public void Include<T>(Action<T> configure = null) where T : IJasperExtension, new()
        {
            var extension = new T();
            configure?.Invoke(extension);

            Include(extension);
        }

    }
}
