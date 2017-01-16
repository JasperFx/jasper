using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Baseline.Conversion;

namespace Jasper.Binding
{
    public abstract class NestedMember<T> : IBoundMember
    {
        private readonly Binder<T> _binder;

        protected NestedMember(Conversions conversions, MemberInfo member, Type memberType)
        {
            Member = member;
            MemberType = memberType;

            _binder = new Binder<T>(conversions);
        }





        public MemberInfo Member { get; }
        public Type MemberType { get; }

        public Expression ToBindExpression(Expression target, Expression source, Expression log, Conversions conversions)
        {
            var fetchSource = Expression.Call(source, BindingExpressions.DataSourceGetChild,
                Expression.Constant(Member.Name));

            var innerSource = Expression.Variable(typeof(IDataSource), "innerSource");
            var assignSource = Expression.Assign(innerSource, fetchSource);

            
            var innerTarget = Expression.Variable(typeof(T), "innerTarget");
            var assignTarget = Expression.Assign(innerTarget, _binder.NewUpExpression);

            var setter = toSetter(target, innerTarget);

            var binders = _binder.Members.Select(x => x.ToBindExpression(innerTarget, innerSource, log, conversions));

            var statements = new Expression[] {assignSource, assignTarget, setter}.Concat(binders);

            var body = Expression.Block(new ParameterExpression[] {innerSource, innerTarget},statements);


            var condition = Expression.Call(source, BindingExpressions.DataSourceHasChild,
                Expression.Constant(Member.Name));


            return Expression.IfThen(condition, body);

        }

        protected abstract Expression toSetter(Expression target, Expression value);

    }
}