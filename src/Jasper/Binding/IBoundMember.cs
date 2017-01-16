using System;
using System.Linq.Expressions;
using System.Reflection;
using Baseline.Conversion;

namespace Jasper.Binding
{
    public interface IBoundMember
    {
        MemberInfo Member { get; }

        Type MemberType { get; }

        Expression ToBindExpression(Expression target, Expression source, Expression log, Conversions conversions);
    }
}