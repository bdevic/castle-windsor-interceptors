using System;
using System.Reflection;

namespace Castle.Windsor.Interceptors.Facility.Registration
{
    public class InterceptorMapping
    {
        public InterceptorMapping(Func<Type, bool> typeSelector, Func<MethodInfo, bool> methodSelector, Type interceptorType,
                                  bool autoRegister = true)
        {
            TypeSelector = typeSelector;
            MethodSelector = methodSelector;
            InterceptorType = interceptorType;
            AutoRegister = autoRegister;
        }

        public InterceptorMapping(Func<Type, bool> typeSelector, Func<MethodInfo, bool> methodSelector, Type interceptorType,
                                  string interceptorKey)
        {
            TypeSelector = typeSelector;
            MethodSelector = methodSelector;
            InterceptorType = interceptorType;
            InterceptorKey = interceptorKey;
            AutoRegister = false;
        }

        public InterceptorMapping()
        {
        }

        public bool AutoRegister { get; protected internal set; }
        public Func<Type, bool> TypeSelector { get; protected internal set; }
        public Func<MethodInfo, bool> MethodSelector { get; protected internal set; }
        public Type InterceptorType { get; protected internal set; }
        public string InterceptorKey { get; protected internal set; }

        public bool Equals(InterceptorMapping other)
        {
            if (ReferenceEquals(null, other)) {
                return false;
            }
            if (ReferenceEquals(this, other)) {
                return true;
            }
            return Equals(other.InterceptorType, InterceptorType) && Equals(other.InterceptorKey, InterceptorKey);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }
            if (ReferenceEquals(this, obj)) {
                return true;
            }
            if (obj.GetType() != typeof (InterceptorMapping)) {
                return false;
            }
            return Equals((InterceptorMapping) obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                return ((InterceptorType != null ? InterceptorType.GetHashCode() : 0)*397) ^
                       (InterceptorKey != null ? InterceptorKey.GetHashCode() : 0);
            }
        }
    }
}