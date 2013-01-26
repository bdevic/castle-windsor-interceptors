using System.Collections.Generic;
using Castle.DynamicProxy;

namespace Castle.Windsor.Interceptors.Tests.Demo
{
    public class StalkingInterceptor : IInterceptor
    {
        public StalkingInterceptor()
        {
            Invocations = new List<IInvocation>();
        }

        public IList<IInvocation> Invocations { get; set; }

        #region IInterceptor Members

        public void Intercept(IInvocation invocation)
        {
            Invocations.Add(invocation);
        }

        #endregion
    }
}