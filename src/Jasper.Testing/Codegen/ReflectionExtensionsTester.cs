using System.Threading.Tasks;
using Baseline.Reflection;
using Jasper.Codegen;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Codegen
{
    public class ReflectionExtensionsTester
    {
        [Fact]
        public void not_async()
        {
            ReflectionHelper.GetMethod<ReflectionExtensionsTester>(x => x.Go())
                .IsAsync().ShouldBeFalse();

            ReflectionHelper.GetMethod<ReflectionExtensionsTester>(x => x.IsTrue())
                .IsAsync().ShouldBeFalse();
        }

        [Fact]
        public void is_async()
        {
            ReflectionHelper.GetMethod<ReflectionExtensionsTester>(x => x.DoAsync())
                .IsAsync().ShouldBeTrue();

            ReflectionHelper.GetMethod<ReflectionExtensionsTester>(x => x.IsTrueAsync())
                .IsAsync().ShouldBeTrue();
        }

        public void Go()
        {

        }

        public bool IsTrue()
        {
            return true;
        }

        public Task DoAsync()
        {
            return Task.CompletedTask;
        }

        public Task<bool> IsTrueAsync()
        {
            return Task.FromResult(true);
        }
    }
}