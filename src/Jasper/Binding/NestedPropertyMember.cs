using System.Linq.Expressions;
using System.Reflection;
using Baseline.Conversion;

namespace Jasper.Binding
{
    public class NestedPropertyMember<T> : NestedMember<T>
    {
        private readonly PropertyInfo _property;

        public NestedPropertyMember(Conversions conversions, PropertyInfo property)
            : base(conversions, property, property.PropertyType)
        {
            _property = property;
        }

        protected override Expression toSetter(Expression target, Expression value)
        {
            var method = _property.SetMethod;

            return Expression.Call(target, method, value);
        }
    }
}