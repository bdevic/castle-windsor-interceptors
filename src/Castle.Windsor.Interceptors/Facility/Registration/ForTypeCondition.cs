using System;

namespace Castle.Windsor.Interceptors.Facility.Registration
{
    public class ForTypeCondition
    {
        private readonly InterceptorRegistration registration;

        public ForTypeCondition(InterceptorRegistration registration)
        {
            this.registration = registration;
        }

        public void Any()
        {
            Where(type => true);
        }

        public void WithAttribute<T>() where T : Attribute
        {
            WithAttribute(typeof (T));
        }

        public void WithAttribute(Type attributeType)
        {
            Where(type => type.GetCustomAttributes(attributeType, true).Length > 0);
        }

        public void Where(Func<Type, bool> condition)
        {
            registration.TypeSelector = condition;
        }
    }
}