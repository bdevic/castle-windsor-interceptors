using System;
using Castle.DynamicProxy;
using Castle.MicroKernel.Registration;
using Castle.Windsor.Interceptors.Aspects;
using Castle.Windsor.Interceptors.Facility;
using Castle.Windsor.Interceptors.Facility.Registration;
using Castle.Windsor.Interceptors.Tests.Demo;
using Common.Logging;
using Moq;
using NUnit.Framework;

namespace Castle.Windsor.Interceptors.Tests
{
    public class TracingInterceptorTest
    {
        private IWindsorContainer container;
        private TracingInterceptor tracingInterceptor;
        
        private Mock<ILog> mockLogger;

        [SetUp]
        public void Setup()
        {
            mockLogger = new Mock<ILog>();
            mockLogger.Setup(x => x.IsTraceEnabled).Returns(true);
            tracingInterceptor = new TracingInterceptor(mockLogger.Object);

            container = new WindsorContainer();
            container.AddFacility<InterceptorFacility>(
                f => f.Register(
                    Interceptor
                        .ForType(x => x.Where(TracedAttribute.TypeSelector))
                        .AndMethod(x => x.Where(TracedAttribute.MethodSelector))
                        .UseRegistered<TracingInterceptor>("tracingInterceptor")));
            container.Register(Component.For<IService>().ImplementedBy<Service1>().Forward<Service1>());
            container.Register(Component.For<IInterceptor>().Instance(tracingInterceptor).Named("tracingInterceptor"));
        }

        [Test]
        public void ShouldLogTraceMessagesOnEnterAndExit()
        {
            // arrange


            // act
            container.Resolve<IService>().Method1("Test");

            // assert
            mockLogger.Verify(x => x.Trace(" Entering: Service1.Method1(Test)"), Times.Once());
            mockLogger.Verify(x => x.Trace(" Exiting: Service1.Method1 [Hello Test from service1.method1]"), Times.Once());
        }

        [Test]
        public void ShouldLogTraceMessagesOnEnterAndException()
        {
            // arrange


            // act
            try
            {
                container.Resolve<IService>().Method1(null);
            }
            catch (Exception)
            { }

            // assert
            mockLogger.Verify(x => x.Trace(" Entering: Service1.Method1(null)"), Times.Once());
            mockLogger.Verify(x => x.Trace(It.Is<string>(a => a.StartsWith(" Exception: Service1.Method1, exception message: No nulls allowed")), It.IsAny<Exception>()), Times.Once());
        }


        [Test]
        public void ShouldBeAbleToCustomiseLogTraceMessages()
        {
            // arrange
            tracingInterceptor.ExitMessage = "${indent} ${returnType} ${classNameFull}.${methodName}(${argumentTypes})(${arguments}) [${returnValue}]";

            // act
            try
            {
                container.Resolve<IService>().Method1("Test");
            }
            catch (Exception)
            { }

            // assert
            mockLogger.Verify(x => x.Trace(" String Castle.Windsor.Interceptors.Tests.Demo.Service1.Method1(String)(Test) [Hello Test from service1.method1]"), Times.Once());
        }

    }
}