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
        
        
        protected sealed override IResolver buildResolver(Scope rootScope)
        {
            if (_resolverType != null)
            {
                return (IResolver) rootScope.QuickBuild(_resolverType.CompiledType);
            }
            
            return null;
        }

        public override string BuildPlan
        {
            get
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