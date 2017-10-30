using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Baseline;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using GenericEnumerableExtensions = Jasper.Internals.Util.GenericEnumerableExtensions;
using StringExtensions = Jasper.Internals.Util.StringExtensions;

#if !NET46

#endif

namespace Jasper.Internals.Compilation
{
    public class AssemblyGenerator
    {
        private static readonly AssemblyLoadContext _context = new CustomAssemblyLoadContext();

        private readonly IList<MetadataReference> _references = new List<MetadataReference>();
        private readonly IList<Assembly> _assemblies = new List<Assembly>();

        public static string[] HintPaths { get; set; }

        public AssemblyGenerator()
        {
            ReferenceAssemblyContainingType<object>();
            ReferenceAssembly(typeof(Enumerable).GetTypeInfo().Assembly);
        }

        public void ReferenceAssembly(Assembly assembly)
        {
            if (assembly == null) return;

            if (_assemblies.Contains(assembly)) return;

            _assemblies.Add(assembly);

            try
            {
                var referencePath = CreateAssemblyReference(assembly);

                if (referencePath == null)
                {
                    Console.WriteLine($"Could not make an assembly reference to {assembly.FullName}");
                    return;
                }

                var alreadyReferenced = _references.Any(x => x.Display == referencePath);
                if (alreadyReferenced)
                    return;

                var reference = MetadataReference.CreateFromFile(referencePath);

                _references.Add(reference);

                foreach (var assemblyName in assembly.GetReferencedAssemblies())
                {
                    var referencedAssembly = Assembly.Load(assemblyName);
                    ReferenceAssembly(referencedAssembly);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not make an assembly reference to {assembly.FullName}\n\n{e}");
            }
        }

        private static string CreateAssemblyReference(Assembly assembly)
        {
            return string.IsNullOrEmpty(assembly.Location)
                ? GetPath(assembly)
                : assembly.Location;
        }

        private static string GetPath(Assembly assembly)
        {
            return HintPaths?
                .Select(FindFile(assembly))
                .FirstOrDefault(file => StringExtensions.IsNotEmpty(file));
        }

        private static Func<string, string> FindFile(Assembly assembly)
        {
            return hintPath =>
            {
                var name = assembly.GetName().Name;
                Console.WriteLine($"Find {name}.dll in {hintPath}");
                var files = Directory.GetFiles(hintPath, name + ".dll", SearchOption.AllDirectories);
                var firstOrDefault = files.FirstOrDefault();
                if (firstOrDefault != null)
                {
                    Console.WriteLine($"Found {name}.dll in {firstOrDefault}");
                }
                return firstOrDefault;
            };
        }

        public void ReferenceAssemblyContainingType<T>()
        {
            ReferenceAssembly(typeof(T).GetTypeInfo().Assembly);
        }

        public Assembly Generate(string code)
        {
            var assemblyName = Path.GetRandomFileName();
            var syntaxTree = CSharpSyntaxTree.ParseText(code);

            var references = _references.ToArray();
            var compilation = CSharpCompilation.Create(assemblyName, new[] { syntaxTree }, references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));


            using (var stream = new MemoryStream())
            {
                var result = compilation.Emit(stream);
                if (!result.Success)
                {
                    new FileSystem().WriteStringToFile("code.txt", code);

                    var failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);


                    var message = GenericEnumerableExtensions.Join(failures.Select(x => $"{x.Id}: {x.GetMessage()}"), "\n");


                    throw new InvalidOperationException("Compilation failures!\n\n" + message + "\n\nCode:\n\n" + code);


                }

                stream.Seek(0, SeekOrigin.Begin);

#if NET46
                return Assembly.Load(stream.ReadAllBytes());
#else
                return _context.LoadFromStream(stream);
#endif

                //                var loader = new InteractiveAssemblyLoader();
                //
                //                var method = loader.GetType()
                //                    .GetMethod("LoadAssemblyFromStream", BindingFlags.NonPublic | BindingFlags.Instance);
                //
                //                return method.Invoke(loader, new object[] {stream, null}).As<Assembly>();
            }
        }
    }

#if !NET46
    public class CustomAssemblyLoadContext : System.Runtime.Loader.AssemblyLoadContext
    {
        protected override Assembly Load(AssemblyName assemblyName)
        {
            return Assembly.Load(assemblyName);
        }
    }
#endif
}
