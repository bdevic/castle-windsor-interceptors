using Castle.DynamicProxy;

namespace Castle.Windsor.Interceptors.Tests.Demo
{
    public class ModifyingInterceptor: IInterceptor
    {
        public string NewArgument { get; set; }
        public void Intercept(IInvocation invocation)
        {
            invocation.Arguments[0] = NewArgument;

            invocation.Proceed();
        }
    }
}