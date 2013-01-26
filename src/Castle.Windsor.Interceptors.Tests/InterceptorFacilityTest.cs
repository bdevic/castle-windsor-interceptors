using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy;
using Castle.MicroKernel.Registration;
using Castle.Windsor.Interceptors.Aspects;
using Castle.Windsor.Interceptors.Facility;
using Castle.Windsor.Interceptors.Facility.Registration;
using Castle.Windsor.Interceptors.Tests.Demo;
using FluentAssertions;
using NUnit.Framework;

namespace Castle.Windsor.Interceptors.Tests
{
    public class InterceptorFacilityTest
    {
        [Test]
        public void ShouldBeAbleToFilterTypesBasedOnSimpleSelector()
        {
            // arrange
            IWindsorContainer container = ContainerFor(Interceptor.ForType<Service2>().AndMethod(x => x.Any()).Use<StalkingInterceptor>());

            // act
            InvokeAllServices(container);

            // assert
            var interceptor = container.Resolve<StalkingInterceptor>();
            interceptor.Invocations.Count.Should().Be(2);
            interceptor.Invocations.Should().OnlyContain(x => x.TargetType == typeof (Service2));
            interceptor.Invocations.Select(x => x.Method.Name).Should().BeEquivalentTo(new List<string> {"Method1", "Method2"});
        }

        [Test]
        public void ShouldBeAbleToFilterTypesBasedOnAttributeSelector()
        {
            // arrange
            IWindsorContainer container =
                ContainerFor(Interceptor.ForType(x => x.WithAttribute<TracedAttribute>()).AndMethod(x => x.Any()).Use<StalkingInterceptor>());

            // act
            InvokeAllServices(container);

            // assert
            var interceptor = container.Resolve<StalkingInterceptor>();
            interceptor.Invocations.Count.Should().Be(2);
            interceptor.Invocations.Should().OnlyContain(x => x.TargetType == typeof (Service1));
            interceptor.Invocations.Select(x => x.Method.Name).Should().BeEquivalentTo(new List<string> {"Method1", "Method2"});
        }

        [Test]
        public void ShouldBeAbleToFilterTypesBasedOnPredicateSelector()
        {
            // arrange
            IWindsorContainer container =
                ContainerFor(Interceptor.ForType(x => x.Where(type => type.Name == "Service3")).AndMethod(x => x.Any()).Use<StalkingInterceptor>());

            // act
            InvokeAllServices(container);

            // assert
            var interceptor = container.Resolve<StalkingInterceptor>();
            interceptor.Invocations.Count.Should().Be(2);
            interceptor.Invocations.Should().OnlyContain(x => x.TargetType == typeof (Service3));
            interceptor.Invocations.Select(x => x.Method.Name).Should().BeEquivalentTo(new List<string> {"Method1", "Method2"});
        }


        [Test]
        public void ShouldBeAbleToFilterMethodsBasedOnSimpleSelector()
        {
            // arrange
            IWindsorContainer container = ContainerFor(Interceptor.ForType(x => x.Any()).AndMethod("Method2").Use<StalkingInterceptor>());

            // act
            InvokeAllServices(container);

            // assert
            var interceptor = container.Resolve<StalkingInterceptor>();
            interceptor.Invocations.Count.Should().Be(3);
            interceptor.Invocations.Should().OnlyContain(x => x.Method.Name == "Method2");
        }

        [Test]
        public void ShouldBeAbleToFilterMethodsBasedOnAttributeSelector()
        {
            // arrange
            IWindsorContainer container =
                ContainerFor(Interceptor.ForType(x => x.Any()).AndMethod(x => x.WithAttribute<TracedAttribute>()).Use<StalkingInterceptor>());

            // act
            InvokeAllServices(container);

            // assert
            var interceptor = container.Resolve<StalkingInterceptor>();
            interceptor.Invocations.Count.Should().Be(1);
            interceptor.Invocations.Should().OnlyContain(x => x.Method.Name == "Method1");
        }

        [Test]
        public void ShouldBeAbleToFilterMethodsBasedOnPredicateSelector()
        {
            // arrange
            IWindsorContainer container =
                ContainerFor(Interceptor.ForType(x => x.Any()).AndMethod(x => x.Where(method => method.Name.EndsWith("2"))).Use<StalkingInterceptor>());

            // act
            InvokeAllServices(container);

            // assert
            var interceptor = container.Resolve<StalkingInterceptor>();
            interceptor.Invocations.Count.Should().Be(3);
            interceptor.Invocations.Should().OnlyContain(x => x.Method.Name == "Method2");
        }

        [Test]
        public void ShouldBeAbleToUsePreconfiguredInterceptors()
        {
            // arrange
            IWindsorContainer container =
                ContainerFor(Interceptor.ForType(x => x.Any()).AndMethod(x => x.Any()).UseRegistered<ModifyingInterceptor>("myInterceptor"));
            var interceptor = new ModifyingInterceptor {NewArgument = "MyInterceptor"};
            container.Register(
                Component.For<ModifyingInterceptor>().ImplementedBy<ModifyingInterceptor>().Named("otherInterceptor"),
                Component.For<ModifyingInterceptor>().Instance(interceptor).Named("myInterceptor")
                );

            // act
            var result = container.Resolve<Service1>().Method1("Test");

            // assert
            result.Should().Be("Hello MyInterceptor from service1.method1");
        }

        private static IWindsorContainer ContainerFor(params InterceptorRegistration[] registrations)
        {
            var container = new WindsorContainer();

            container.AddFacility<InterceptorFacility>(f => f.Register(registrations));

            container.Register(Component.For<IService>().ImplementedBy<Service1>().Forward<Service1>());
            container.Register(Component.For<IService>().ImplementedBy<Service2>().Forward<Service2>());
            container.Register(Component.For<IService>().ImplementedBy<Service3>().Forward<Service3>());
            return container;
        }

        private void InvokeAllServices(IWindsorContainer container)
        {
            IService[] services = container.ResolveAll<IService>();
            foreach (IService service in services)
            {
                service.Method1("Test");
                service.Method2("Test");
            }
        }
    }
}