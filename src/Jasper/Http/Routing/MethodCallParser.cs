using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Jasper.Http.Routing
{
    public class MethodCallParser : ExpressionVisitor
    {
        internal readonly List<object> Arguments = new List<object>();

        private MethodCallParser()
        {
        }

        public static List<object> ToArguments(Expression expression)
        {
            var parser = new MethodCallParser();
            parser.Visit(expression);

            return parser.Arguments;
        }

        public static List<object> ToArguments<T>(Expression<Action<T>> expression)
        {
            var parser = new MethodCallParser();
            parser.Visit(expression);

            return parser.Arguments;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            Arguments.Add(node.Value);

            return base.VisitConstant(node);
        }

        protected override Expression VisitMember(MemberExpression member)
        {
            if (member.Expression is ConstantExpression &&
                member.Member is FieldInfo)
            {
                var container =
                    ((ConstantExpression) member.Expression).Value;
                var value = ((FieldInfo) member.Member).GetValue(container);

                Arguments.Add(value);

                return member;
            }

            return base.VisitMember(member);
        }
    }
}
