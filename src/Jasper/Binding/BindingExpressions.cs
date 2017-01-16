using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Baseline.Conversion;

namespace Jasper.Binding
{
    public class BindingExpressions
    {
        internal static MethodInfo DataSourceGet = typeof(IDataSource).GetMethod(nameof(IDataSource.Get));
        internal static MethodInfo DataSourceGetChild = typeof(IDataSource).GetMethod(nameof(IDataSource.GetChild));
        internal static MethodInfo DataSourceHas = typeof(IDataSource).GetMethod(nameof(IDataSource.Has));
        internal static MethodInfo DataSourceHasChild = typeof(IDataSource).GetMethod(nameof(IDataSource.HasChild));

        public static Dictionary<Type, Func<Expression, Expression>> Conversions = new Dictionary<Type, Func<Expression, Expression>>();


        static BindingExpressions()
        {
            parse<int>();
            parse<bool>();
            parse<byte>();
            parse<char>();
            parse<decimal>();
            parse<float>();
            parse<short>();
            parse<long>();
            parse<ushort>();
            parse<ulong>();
            parse<Guid>();
        }

        private static void parse<T>()
        {
            var method = typeof(T).GetMethod(nameof(int.Parse), new Type[] {typeof(string)});

            Conversions.Add(typeof(T), value => Expression.Call(null, method, value));

            
        }

        internal static Expression ToConversion(Conversions conversions, Expression value, Type memberType)
        {
            if (memberType == typeof(string))
            {
                return value;
            }

            if (Conversions.ContainsKey(memberType))
            {
                return Conversions[memberType](value);
            }

            var func = conversions.FindConverter(memberType);

            value = Expression.Invoke(Expression.Constant(func), value);
            value = Expression.Convert(value, memberType);

            return value;
        }
    }
}