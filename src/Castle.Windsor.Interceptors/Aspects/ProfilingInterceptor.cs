using System;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;
using Common.Logging;

namespace Castle.Windsor.Interceptors.Aspects
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, Inherited = true)]
    public class ProfiledAttribute : Attribute
    {
        public static Func<Type, bool> TypeSelector = type => type.GetCustomAttributes(typeof (ProfiledAttribute), true).Length > 0;

        public static Func<MethodInfo, bool> MethodSelector =
            method =>
            method.IsPublic && !method.IsSpecialName &&
            (
                method.GetCustomAttributes(typeof (ProfiledAttribute), true).Length > 0 ||
                method.DeclaringType.GetCustomAttributes(typeof (ProfiledAttribute), true).Count(x => ((ProfiledAttribute) x).IncludePublic) > 0
            );

        private bool includePublic = true;

        /// <summary>
        /// Automatically log all public methods declared under this type.
        /// </summary>
        public bool IncludePublic
        {
            get { return includePublic; }
            set { includePublic = value; }
        }
    }

    public class ProfilingInterceptor : IInterceptor
    {
        private static readonly ILog Logger = LogManager.GetLogger<ProfilingInterceptor>();

        #region IInterceptor Members

        public void Intercept(IInvocation invocation)
        {
            DateTime invocationStart = DateTime.Now;

            try {
                invocation.Proceed();
            }
            finally {
                Logger.Trace(
                    m =>
                    m("Execution time: {0}.{1} - {2}ms", invocation.TargetType.FullName, invocation.Method.Name,
                      (DateTime.Now - invocationStart).TotalMilliseconds));
            }
        }

        #endregion
    }
}