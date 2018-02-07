using System;
using System.Linq;
using Baseline;
using BlueMilk.Codegen;
using BlueMilk.Codegen.Frames;
using BlueMilk.Codegen.Variables;
using BlueMilk.Compilation;
using BlueMilk.IoC.Frames;
using BlueMilk.IoC.Resolvers;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMilk.IoC.Instances
{
    public abstract class GeneratedInstance : Instance
    {
        private GeneratedType _resolverType;

        protected GeneratedInstance(Type serviceType, Type implementationType, ServiceLifetime lifetime) : base(serviceType, implementationType, lifetime)
        {
        }


        public void GenerateResolver(GeneratedAssembly generatedAssembly)
        {
            if (ErrorMessages.Any() || Dependencies.SelectMany(x => x.ErrorMessages).Any()) return;

            var typeName = (ServiceType.FullNameInCode() + "_" + Name).Replace('<', '_').Replace('>', '_').Replace(" ", "")
                .Replace(',', '_').Replace('.', '_').Replace("[", "").Replace("]", "");

            _resolverType = generatedAssembly.AddType(typeName, ResolverBaseType.MakeGenericType(ServiceType));

            var method = _resolverType.MethodFor("Build");

            var frame = CreateBuildFrame();

            method.Frames.Add(frame);
        }

        public void AttachResolver(Scope root)
        {
            if (ErrorMessages.Any() || Dependencies.Any(x => x.ErrorMessages.Any()))
            {
                _resolver = new ErrorMessageResolver(this);
            }
            else
            {
                _resolver = (IResolver) root.QuickBuild(_resolverType.CompiledType);
            }

            _resolver.Hash = GetHashCode();
            _resolver.Name = Name;
        }

        public sealed override Variable CreateVariable(BuildMode mode, ResolverVariables variables, bool isRoot)
        {
            if (Lifetime == ServiceLifetime.Singleton)
                return mode == BuildMode.Build
                    ? generateVariableForBuilding(variables, mode, isRoot)
                    : new InjectedServiceField(this);




            if (Lifetime == ServiceLifetime.Scoped && mode == BuildMode.Dependency)
            {
                return new GetInstanceFrame(this).Variable;
            }



            return generateVariableForBuilding(variables, mode, isRoot);
        }

        public abstract Frame CreateBuildFrame();
        protected abstract Variable generateVariableForBuilding(ResolverVariables variables, BuildMode mode, bool isRoot);

        private readonly object _locker = new object();
        protected IResolver _resolver;

        public override object Resolve(Scope scope)
        {
            if (_resolver == null)
            {
                lock (_locker)
                {
                    if (_resolver == null)
                    {
                        if (ErrorMessages.Any() || Dependencies.Any(x => x.ErrorMessages.Any()))
                        {
                            _resolver = new ErrorMessageResolver(this);
                        }
                        else
                        {
                            var assembly = scope.ServiceGraph.ToGeneratedAssembly();
                            GenerateResolver(assembly);

                            if (_resolverType == null)
                            {
                                _resolver = new ErrorMessageResolver(this);
                            }
                            else
                            {
                                assembly.CompileAll();

                                _resolver = (IResolver) scope.Root.QuickBuild(_resolverType.CompiledType);
                            }
                        }

                        _resolver.Hash = GetHashCode();
                        _resolver.Name = Name;
                    }
                }
            }


            return _resolver.Resolve(scope);
        }

        public override string GetBuildPlan()
        {
            if (_resolverType != null)
            {
                return _resolverType.SourceCode;
            }

            if (ErrorMessages.Any())
            {
                return "Errors!" + Environment.NewLine + ErrorMessages.Join(Environment.NewLine);
            }

            return ToString();
        }

        public Type ResolverBaseType
        {
            get
            {
                switch (Lifetime)
                {
                    case ServiceLifetime.Scoped:
                        return typeof(ScopedResolver<>);

                    case ServiceLifetime.Singleton:
                        return typeof(SingletonResolver<>);

                    case ServiceLifetime.Transient:
                        return typeof(TransientResolver<>);
                }

                return null;
            }
        }
    }
}
