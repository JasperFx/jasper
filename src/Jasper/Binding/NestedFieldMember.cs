using System.Linq.Expressions;
using System.Reflection;
using Baseline.Conversion;

namespace Jasper.Binding
{
    public class NestedFieldMember<T> : NestedMember<T>
    {
        private readonly FieldInfo _field;

        public NestedFieldMember(Conversions conversions, FieldInfo field) 
            : base(conversions, field, field.FieldType)
        {
            _field = field;
        }

        protected override Expression toSetter(Expression target, Expression value)
        {
            var fieldExpression = Expression.Field(target, _field);
            return Expression.Assign(fieldExpression, value);
        }
    }
}