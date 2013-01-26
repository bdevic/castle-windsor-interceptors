using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;
using Castle.Windsor.Interceptors.Facility.Registration;
using Common.Logging;

namespace Castle.Windsor.Interceptors.Facility
{
    public class InterceptorSelector : IInterceptorSelector
    {
        private static readonly ILog Logger = LogManager.GetLogger<InterceptorSelector>();

        public InterceptorSelector(IList<InterceptorMapping> interceptorMappings)
        {
            InterceptorMappings = interceptorMappings;
        }

        public IList<InterceptorMapping> InterceptorMappings { get; private set; }

        #region IInterceptorSelector Members

        public IInterceptor[] SelectInterceptors(Type type, MethodInfo method, IInterceptor[] interceptors)
        {
            Logger.Trace(m => m("Called interceptor selector for method {0}.{1} and interceptors {2}",
                                type.FullName,
                                method.Name,
                                interceptors
                                    .Select(i => i.GetType().Name)
                                    .Aggregate((s1, s2) => s1 + " " + s2)));

            var applicableMappings = GetInterceptorsApplicableTo(type, method);
            var applicableInterceptors = interceptors.Where(x => applicableMappings.Any(m => m.InterceptorType == x.GetType())).ToArray();
                   
            Logger.TraceFormat("Applying {0} interceptors[{1}] to method {2}.{3}...",
                               applicableInterceptors.Length,
                               string.Join(", ", applicableInterceptors.Select(x => x.GetType().Name).ToArray()),
                               type.FullName, method.Name);

            return applicableInterceptors;
        }

        protected ICollection<InterceptorMapping> GetInterceptorsApplicableTo(Type type, MethodInfo method)
        {
            MethodInfo methodImpl = null;
            if (method.DeclaringType != null && method.DeclaringType != type)
            {
                methodImpl = type.GetMethod(method.Name, method.GetParameters().Select(x => x.ParameterType).ToArray());
            }

            return InterceptorMappings.Where(x => (methodImpl != null && x.MethodSelector(methodImpl)) || x.MethodSelector(method)).ToList();
        }

        #endregion
    }
}