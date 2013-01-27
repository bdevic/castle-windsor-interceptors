using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Castle.DynamicProxy;
using Common.Logging;

namespace Castle.Windsor.Interceptors.Aspects
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, Inherited = true)]
    public class TracedAttribute : Attribute
    {
        public static Func<Type, bool> TypeSelector = type => type.GetCustomAttributes(typeof (TracedAttribute), true).Length > 0;

        public static Func<MethodInfo, bool> MethodSelector =
            method =>
            method.IsPublic && !method.IsSpecialName &&
            (
                method.GetCustomAttributes(typeof (TracedAttribute), true).Length > 0 ||
                method.DeclaringType.GetCustomAttributes(typeof (TracedAttribute), true).Count(x => ((TracedAttribute) x).IncludePublic) > 0
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

    public class TracingInterceptor : IInterceptor
    {
        public const string VAR_RETURN_TYPE = "${returnType}";
        public const string VAR_METHOD_NAME = "${methodName}";
        public const string VAR_ARGUMENT_TYPES = "${argumentTypes}";
        public const string VAR_CLASS_NAME = "${className}";
        public const string VAR_CLASS_NAME_FULL = "${classNameFull}";
        public const string VAR_RETURN_VALUE = "${returnValue}";
        public const string VAR_ARGUMENTS = "${arguments}";
        public const string VAR_EXCEPTION = "${exception}";
        public const string VAR_EXCEPTION_MESSAGE = "${exceptionMsg}";
        public const string VAR_INDENT = "${indent}";

        protected ILog DefaultLogger;

        public TracingInterceptor(): this(LogManager.GetLogger<TracingInterceptor>())
        {
        }

        public TracingInterceptor(ILog defaultLogger)
        {
            this.DefaultLogger = defaultLogger;
            this.EnterMessage = "${indent} Entering: ${className}.${methodName}(${arguments})";
            this.ExitMessage = "${indent} Exiting: ${className}.${methodName} [${returnValue}]";
            this.ExceptionMessage = "${indent} Exception: ${className}.${methodName}, exception message: ${exceptionMsg}";
        }

        [ThreadStatic] protected static int InvocationLevel = -1;

        public string EnterMessage { get; set; }
        public string ExitMessage { get; set; }
        public string ExceptionMessage { get; set; }

        #region IInterceptor Members

        public void Intercept(IInvocation invocation)
        {
            ILog logger = GetLoggerFor(invocation);

            if (logger.IsTraceEnabled) {
                InvokeWithTrace(logger, invocation);
            }
            else {
                invocation.Proceed();
            }
        }

        #endregion

        protected ILog GetLoggerFor(IInvocation invocation)
        {
            return DefaultLogger;
        }

        protected void InvokeWithTrace(ILog logger, IInvocation invocation)
        {
            ++InvocationLevel;

            try {
                if (!string.IsNullOrEmpty(EnterMessage)) {
                    logger.Trace(GetFormattedMessage(EnterMessage, invocation));
                }

                invocation.Proceed();

                if (!string.IsNullOrEmpty(ExitMessage)) {
                    logger.Trace(GetFormattedMessage(ExitMessage, invocation));
                }
            }
            catch (Exception e) {
                if (!string.IsNullOrEmpty(ExceptionMessage)) {
                    logger.Trace(GetFormattedMessage(ExceptionMessage, invocation, e), e);
                }
                throw;
            }
            finally {
                --InvocationLevel;
            }
        }

        protected string GetFormattedMessage(string message, IInvocation invocation, Exception exception = null)
        {
            var buildy = new StringBuilder(message);

            buildy.Replace(VAR_ARGUMENTS, string.Join(", ", invocation.Arguments.Select(ToString).ToArray()));
            buildy.Replace(VAR_ARGUMENT_TYPES, string.Join(", ", invocation.Method.GetParameters().Select(x => x.ParameterType.Name)));
            buildy.Replace(VAR_CLASS_NAME, invocation.TargetType.Name);
            buildy.Replace(VAR_CLASS_NAME_FULL, invocation.TargetType.FullName);
            if (exception != null) {
                buildy.Replace(VAR_EXCEPTION_MESSAGE, exception.Message);
                buildy.Replace(VAR_EXCEPTION, exception.ToString());
            }

            buildy.Replace(VAR_INDENT, new string(' ', InvocationLevel*2));
            buildy.Replace(VAR_METHOD_NAME, invocation.Method.Name);
            buildy.Replace(VAR_RETURN_TYPE, invocation.Method.ReturnType.Name);
            if (invocation.ReturnValue != null) {
                buildy.Replace(VAR_RETURN_VALUE, ToString(invocation.ReturnValue));
            }

            return buildy.ToString();
        }

        protected string ToString(object argument)
        {
            if (argument == null) {
                return "null";
            }

            return argument.ToString();
        }
    }
}