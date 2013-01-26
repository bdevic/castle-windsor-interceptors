using System;
using System.Reflection;

namespace Castle.Windsor.Interceptors.Facility.Registration
{
    public class AndMethodCondition
    {
        private readonly InterceptorRegistration registration;

        public AndMethodCondition(InterceptorRegistration registration)
        {
            this.registration = registration;
        }

        public void Any()
        {
            Where(method => true);
        }

        public void WithAttribute<T>() where T : Attribute
        {
            WithAttribute(typeof (T));
        }

        public void WithAttribute(Type attributeType)
        {
            Where(method => method.GetCustomAttributes(attributeType, true).Length > 0);
        }

        public void Where(Func<MethodInfo, bool> condition)
        {
            registration.MethodSelector = condition;
        }
    }
}