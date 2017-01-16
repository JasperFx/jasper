using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Baseline;
using Baseline.Conversion;
using LogProblem = System.Action<System.Reflection.MemberInfo, System.Exception>;

namespace Jasper.Binding
{
    public class Binder<T>
    {
        private readonly Action<IDataSource, T, LogProblem> _bindAll;
        private readonly Func<T> _create;

        private readonly IList<IBoundMember> _members = new List<IBoundMember>();
        public NewExpression NewUpExpression;

        public Binder() : this(new Conversions())
        {
        }

        public Binder(Conversions conversions)
        {
            var source = Expression.Parameter(typeof(IDataSource), "source");
            var target = Expression.Parameter(typeof(T), "target");
            var log = Expression.Parameter(typeof(LogProblem));

            _members.AddRange(BoundProperty.FindMembers(typeof(T), conversions));
            _members.AddRange(BoundField.FindMembers(typeof(T), conversions));


            var allSetters = _members.Select(x => x.ToBindExpression(target, source, log, conversions)).ToArray();

            var block = Expression.Block(allSetters);

            var bindAll = Expression.Lambda<Action<IDataSource, T, LogProblem>>(block, source, target, log);

            

            _bindAll = bindAll.Compile();

            var ctor = typeof(T).GetConstructors().FirstOrDefault(x => x.GetParameters().Length == 0);
            if (ctor != null)
            {
                NewUpExpression = Expression.New(ctor);
                _create = Expression.Lambda<Func<T>>(NewUpExpression).Compile();
            }
        }

        public IEnumerable<IBoundMember> Members => _members;

        public bool CanBuild => _create != null;


        public void Bind(IDataSource source, T target, LogProblem log = null)
        {
            log = log ?? throwProblem;

            _bindAll(source, target, log);
        }

        private static void throwProblem(MemberInfo member, Exception ex)
        {
            throw new BindingException(member, ex);
        }

        public T Build(IDataSource source, LogProblem log = null)
        {
            if (!CanBuild) throw new InvalidOperationException($"The binder for {typeof(T).FullName} cannot build new objects");

            var target = _create();

            log = log ?? throwProblem;
            _bindAll(source, target, log);

            return target;
        }

    }

    public class BindingException : Exception
    {
        public BindingException(MemberInfo member, Exception inner) : base($"Unable to convert the value of {member.Name}", inner)
        {
        }
    }
}