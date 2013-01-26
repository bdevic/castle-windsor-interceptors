using System;
using System.Reflection;
using Castle.DynamicProxy;

namespace Castle.Windsor.Interceptors.Facility.Registration
{
    public class InterceptorRegistration
    {
        protected internal bool AutoRegister { get; set; }
        protected internal Func<Type, bool> TypeSelector { get; set; }
        protected internal Func<MethodInfo, bool> MethodSelector { get; set; }
        protected internal Type InterceptorType { get; set; }
        protected internal string InterceptorKey { get; set; }

        public InterceptorRegistration ForType<T>() where T : class
        {
            return ForType(typeof (T));
        }

        public InterceptorRegistration ForType(Type type)
        {
            return ForType(x => x.Where(t => t == type));
        }

        public InterceptorRegistration ForType(Action<ForTypeCondition> action)
        {
            var condition = new ForTypeCondition(this);
            action(condition);
            return this;
        }

        public InterceptorRegistration AndMethod(string methodName)
        {
            return AndMethod(x => x.Where(method => method.Name == methodName));
        }

        public InterceptorRegistration AndMethod(Action<AndMethodCondition> action)
        {
            var condition = new AndMethodCondition(this);
            action(condition);
            return this;
        }

        public InterceptorRegistration Use<T>(bool autoRegister = true) where T : IInterceptor
        {
            return Use(typeof (T), autoRegister);
        }

        public InterceptorRegistration Use(Type interceptorType, bool autoRegister = true)
        {
            if (interceptorType == null || !typeof (IInterceptor).IsAssignableFrom(interceptorType))
            {
                throw new ArgumentException("Parameter interceptorType must be a class derived from IInterceptor",
                                            "interceptorType");
            }
            InterceptorType = interceptorType;
            AutoRegister = autoRegister;
            return this;
        }

        public InterceptorRegistration UseRegistered<T>(string interceptorKey) where T : IInterceptor
        {
            return UseRegistered(typeof (T), interceptorKey);
        }

        public InterceptorRegistration UseRegistered(Type interceptorType, string interceptorKey)
        {
            if (string.IsNullOrEmpty(interceptorKey))
            {
                throw new ArgumentException("Parameter interceptorKey must be a valid string", "interceptorKey");
            }

            Use(interceptorType, false);
            InterceptorKey = interceptorKey;
            return this;
        }

        protected internal InterceptorMapping Build()
        {
            return new InterceptorMapping
                       {
                           AutoRegister = AutoRegister,
                           InterceptorKey = InterceptorKey,
                           InterceptorType = InterceptorType,
                           MethodSelector = MethodSelector,
                           TypeSelector = TypeSelector
                       };
        }
    }
}