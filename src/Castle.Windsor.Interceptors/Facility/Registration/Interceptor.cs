using System;

namespace Castle.Windsor.Interceptors.Facility.Registration
{
    public class Interceptor
    {
        public static InterceptorRegistration ForType(Type type)
        {
            return new InterceptorRegistration().ForType(type);
        }

        public static InterceptorRegistration ForType<T>() where T : class
        {
            return new InterceptorRegistration().ForType<T>();
        }

        public static InterceptorRegistration ForType(Action<ForTypeCondition> action)
        {
            return new InterceptorRegistration().ForType(action);
        }
    }
}