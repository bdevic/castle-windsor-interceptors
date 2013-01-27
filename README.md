##castle-windsor-interceptors

This sample shows how to use Castle.DynamicProxy interceptors to implement AOP concepts in 
the Castle.Windsor(v3.1.0.0) container.

The sample contains a facility for fluent interceptor registration and some simple method tracing
and profiling interceptors to showcase class and method filtering through custom attributes.

##Usage
Implement custom interceptor, e.g.

    public class TracingInterceptor: IInterceptor {
    ...
    }

Add the InterceptorFacility to the container and define the pointcuts to which to apply the interceptors

    var container = new WindsorContainer();
    container.AddFacility<InterceptorFacility>(f => f.Register
    (
        Interceptor.ForType(x => x.Any()).AndMethod(x => x.WithAttribute<TransactionAttribute>()).Use<TransactionInterceptor>(),
        Interceptor.ForType(x => x.Where(type => type.Name.EndsWith("Service"))).AndMethod(x => x.Any()).Use<ProfilingInterceptor>(),
        Interceptor.ForType(x => x.Where(TracedAttribute.TypeSelector)).AndMethod(x => x.Any()).UseRegistered<TracingInterceptor>("customTracing")
    ));

    var interceptor = new TracingInterceptor
        {
            EnterMessage = "Entered: ${indent} ${returnType} ${classNameFull}.${methodName}(${arguments})",
            ExitMessage = "Exited: ${indent} ${returnType} ${classNameFull}.${methodName} [${returnValue}]"
        };
    container.Register(Component.For<IInterceptor>().Instance(interceptor).Named("customTracing"));


####Note
For Castle.DynamicProxy interceptors to work, the proxied methods must either be
invoked through an interface or marked as virtual.
