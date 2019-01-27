using System;
using System.Collections.Generic;
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
    public class MessageContextInstance : Instance
    {
        private readonly Type _serviceType;
        private IMessagingRoot _root;
        private Instance _rootInstance;

        public MessageContextInstance(Type serviceType) : base(
            serviceType,
            typeof(MessageContext),
            ServiceLifetime.Transient)
        {
            _serviceType = serviceType;
            Name = Variable.DefaultArgName<IMessageContext>();
        }

        public override bool RequiresServiceProvider => false;

        public override Func<Scope, object> ToResolver(Scope topScope)
        {
            return s => topScope.GetInstance<IMessagingRoot>().NewContext();
        }

        public override object Resolve(Scope scope)
        {
            if (_root == null) _root = scope.GetInstance<IMessagingRoot>();

            return _root.NewContext();
        }

        public override Variable CreateVariable(BuildMode mode, ResolverVariables variables, bool isRoot)
        {
            var root = variables.Resolve(_rootInstance, mode);
            return new NewContextFrame(this, root).Variable;
        }

        protected override IEnumerable<Instance> createPlan(ServiceGraph services)
        {
            _rootInstance = services.FindDefault(typeof(IMessagingRoot));
            yield return _rootInstance;
        }

        public class NewContextFrame : SyncFrame
        {
            private readonly Variable _root;

            public NewContextFrame(MessageContextInstance instance, Variable root)
            {
                _root = root;
                Variable = new ServiceVariable(instance, this);
                uses.Add(root);
            }

            public Variable Variable { get; }

            public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
            {
                writer.Write($"{Variable.VariableType.FullNameInCode()} {Variable.Usage} = {_root.Usage}.{nameof(IMessagingRoot.NewContext)}();");
                Next?.GenerateCode(method, writer);
            }
        }
    }
}
