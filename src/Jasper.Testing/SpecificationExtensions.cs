using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Shouldly;

namespace Jasper.Testing
{
    public static class Exception<T> where T : Exception
    {
        public static T ShouldBeThrownBy(Action action)
        {
            T exception = null;

            try
            {
                action();
            }
            catch (Exception e)
            {
                exception = e.ShouldBeOfType<T>();
            }

            exception.ShouldNotBeNull("An exception was expected, but not thrown by the given action.");

            return exception;
        }

        public static async Task<T> ShouldBeThrownByAsync(Func<Task> action)
        {
            T exception = null;

            try
            {
                await action();
            }
            catch (Exception e)
            {
                exception = e.ShouldBeOfType<T>();
            }

            exception.ShouldNotBeNull("An exception was expected, but not thrown by the given action.");

            return exception;
        }
    }

    public delegate void MethodThatThrows();

    public static class SpecificationExtensions
    {


        public static void ShouldHaveTheSameElementsAs<T>(this IEnumerable<T> actual, IEnumerable<T> expected)
        {
            var actualList = (actual is IList) ? (IList)actual : actual.ToList();
            var expectedList = (expected is IList) ? (IList)expected : expected.ToList();

            ShouldHaveTheSameElementsAs(actualList, expectedList);
        }

        public static void ShouldHaveTheSameElementsAs<T>(this IEnumerable<T> actual, params T[] expected)
        {
            var actualList = (actual is IList) ? (IList)actual : actual.ToList();
            var expectedList = (expected is IList) ? (IList)expected : expected.ToList();

            ShouldHaveTheSameElementsAs(actualList, expectedList);
        }

        public static void ShouldHaveTheSameElementsAs(this IList actual, IList expected)
        {
            actual.ShouldNotBeNull();
            expected.ShouldNotBeNull();

            try
            {
                actual.Count.ShouldBe(expected.Count);

                for (var i = 0; i < actual.Count; i++)
                {
                    actual[i].ShouldBe(expected[i]);
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("ACTUAL:");
                foreach (var o in actual)
                {
                    Debug.WriteLine(o);
                }
                throw;
            }
        }


        public static void ShouldBeNull(this object anObject)
        {
            anObject.ShouldBe(null);
        }

        public static void ShouldNotBeNull(this object anObject)
        {
            anObject.ShouldNotBe(null);
        }

        public static object ShouldBeTheSameAs(this object actual, object expected)
        {
            ReferenceEquals(actual, expected).ShouldBeTrue();
            return expected;
        }

        public static T IsType<T>(this object actual)
        {
            actual.ShouldBeOfType(typeof(T));
            return (T)actual;
        }

        public static object ShouldNotBeTheSameAs(this object actual, object expected)
        {
            ReferenceEquals(actual, expected).ShouldBeFalse();
            return expected;
        }

        public static void ShouldNotBeOfType<T>(this object actual)
        {
            actual.ShouldNotBeOfType(typeof(T));
        }

        public static void ShouldNotBeOfType(this object actual, Type expected)
        {
            actual.GetType().ShouldNotBe(expected);
        }



        public static Exception ShouldBeThrownBy(this Type exceptionType, MethodThatThrows method)
        {
            Exception exception = null;

            try
            {
                method();
            }
            catch (Exception e)
            {
                e.GetType().ShouldBe(exceptionType);
                exception = e;
            }

            exception.ShouldNotBeNull($"Expected {exceptionType.FullName} to be thrown.");


            return exception;
        }

    }
}
