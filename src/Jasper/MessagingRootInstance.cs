using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Baseline.Reflection;
using Jasper.Messaging;
using Lamar;
using Lamar.IoC;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;
using LamarCompiler;
using LamarCompiler.Frames;
using LamarCompiler.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper
{
    public class MessagingRootInstance<T> : Instance
    {
        private readonly PropertyInfo _property;
        private Instance _root;

        public MessagingRootInstance(Expression<Func<IMessagingRoot, T>> expression) : base(typeof(T), typeof(T),
            ServiceLifetime.Singleton)
        {
            _property = ReflectionHelper.GetProperty(expression);
            Name = Variable.DefaultArgName<T>();
        }

        public override bool RequiresServiceProvider => false;

        public override Func<Scope, object> ToResolver(Scope topScope)
        {
            // Not very worried about performance here 'cause this shouldn't get
            // used outside of testing
            return s => _property.GetValue(s.GetInstance<IMessagingRoot>());
        }

        public override object Resolve(Scope scope)
        {
            // Not very worried about performance here 'cause this shouldn't get
            // used outside of testing
            return _property.GetValue(scope.GetInstance<IMessagingRoot>());
        }

        protected override IEnumerable<Instance> createPlan(ServiceGraph services)
        {
            _root = services.FindDefault(typeof(IMessagingRoot));
            yield return _root;
        }

        public override Variable CreateVariable(BuildMode mode, ResolverVariables variables, bool isRoot)
        {
            var root = variables.Resolve(_root, mode);
            return new MessagingRootFrame(this, root).Service;
        }

        public class MessagingRootFrame : SyncFrame
        {
            private readonly MessagingRootInstance<T> _parent;
            private readonly Variable _root;

            public MessagingRootFrame(MessagingRootInstance<T> parent, Variable root)
            {
                _parent = parent;
                _root = root;
                uses.Add(root);
                Service = new ServiceVariable(parent, this);
            }

            public ServiceVariable Service { get; }

            public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
            {
                writer.Write($"var {Service.Usage} = {_root.Usage}.{_parent._property.Name};");
                Next?.GenerateCode(method, writer);
            }
        }
    }
}
