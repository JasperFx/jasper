using System;
using NSubstitute;
using StructureMap;
using StructureMap.AutoMocking;

namespace JasperBus.Tests
{
    public class InteractionContext<T> where T : class
    {
        public InteractionContext()
        {
            Services = new AutoMocker<T>(new NSubstituteServiceLocator());
        }

        public IContainer Container => Services.Container;
        public AutoMocker<T> Services { get; }
        public T ClassUnderTest => Services.ClassUnderTest;


        public TService MockFor<TService>() where TService : class
        {
            return Services.Get<TService>();
        }
    }

    public class NSubstituteServiceLocator : ServiceLocator
    {
        public T Service<T>() where T : class
        {
            return Substitute.For<T>();
        }

        public object Service(Type serviceType)
        {
            return Substitute.For(new[] { serviceType }, new object[0]);
        }

        public T PartialMock<T>(params object[] args) where T : class
        {
            return Substitute.For<T>(args);
        }
    }
}