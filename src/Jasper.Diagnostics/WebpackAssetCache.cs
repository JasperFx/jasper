using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Embedded;
using Newtonsoft.Json;

namespace Jasper.Diagnostics
{
    public class WebpackAssetCache
    {
        private IFileProvider _fileProvider;
        private string[] _files;

        public WebpackAssetCache()
        {
            var assembly = typeof(DiagnosticsMiddleware).GetTypeInfo().Assembly;
            _fileProvider = new EmbeddedFileProvider(assembly, $"{assembly.GetName().Name}.resources");
        }

        public string[] CssFiles()
        {
            return GetFiles().Where(x => x.EndsWith(".css")).ToArray();
        }

        public string[] JavaScriptFiles()
        {
            return GetFiles().Where(x => x.EndsWith(".js")).ToArray();
        }

        public IEnumerable<string> GetFiles()
        {
            if (_files != null)
            {
                return _files;
            }

            var file = _fileProvider.GetFileInfo("manifest.json");
            using(var stream = file.CreateReadStream())
            using(var reader = new StreamReader(stream))
            {
                var json = reader.ReadToEnd();
                var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                _files = values.Select(x => x.Value?.ToString()).ToArray();

                return _files;
            }
        }
    }
}
