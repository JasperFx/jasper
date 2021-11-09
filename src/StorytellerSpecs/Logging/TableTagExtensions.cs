using System;
using System.Linq;
using System.Linq.Expressions;
using Baseline;
using Baseline.Reflection;
using Jasper;
using StoryTeller.Util;

namespace StorytellerSpecs.Logging
{
    public static class TableTagExtensions
    {
        public static void WriteEnvelopeProperty<T>(this TableTag tag, Envelope envelope,
            Expression<Func<Envelope, T>> expression)
        {
            var value = expression.Compile()(envelope);

            if (value == null)
            {
                return;
            }

            if (typeof(T) == typeof(Guid) && Guid.Empty.Equals(value))
            {
                return;
            }

            var text = value.ToString();
            if (value is string[])
            {
                var values = value.As<string[]>();
                if (!values.Any())
                {
                    return;
                }

                text = values.Join(", ");
            }

            tag.AddBodyRow(row =>
            {
                var prop = ReflectionHelper.GetProperty(expression);
                row.Cell(prop.Name);

                row.Cell(text);
            });
        }
    }
}
