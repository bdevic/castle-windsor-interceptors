using System;
using Castle.Windsor.Interceptors.Aspects;

namespace Castle.Windsor.Interceptors.Tests.Demo
{
    public interface IService
    {
        string Method1(string input);
        string Method2(string input);
    }

    [Traced]
    public class Service1 : IService
    {
        #region IService Members

        public virtual string Method1(string input)
        {
            if (input == null)
            {
                throw new InvalidOperationException("No nulls allowed");
            }
            return "Hello " + input + " from service1.method1";
        }

        public string Method2(string input)
        {
            return "Hello " + input + " from service1.method2";
        }

        #endregion
    }

    public class Service2 : IService
    {
        #region IService Members

        [Traced]
        public string Method1(string input)
        {
            return "Hello " + input + " from service2.method1";
        }

        public string Method2(string input)
        {
            return "Hello " + input + " from service2.method2";
        }

        #endregion
    }

    public class Service3 : IService
    {
        #region IService Members

        public string Method1(string input)
        {
            return "Hello " + input + " from service3.method1";
        }

        public string Method2(string input)
        {
            return "Hello " + input + " from service3.method2";
        }

        #endregion
    }
}