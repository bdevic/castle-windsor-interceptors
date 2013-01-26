using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy;
using Castle.MicroKernel.Facilities;
using Castle.MicroKernel.Registration;
using Castle.Windsor.Interceptors.Facility.Registration;

namespace Castle.Windsor.Interceptors.Facility
{
    public class InterceptorFacility : AbstractFacility
    {
        private IList<InterceptorMapping> interceptorMappings;

        public void Register(params InterceptorMapping[] mappings)
        {
            interceptorMappings = new List<InterceptorMapping>(mappings);
        }

        public void Register(params InterceptorRegistration[] registrations)
        {
            interceptorMappings = new List<InterceptorMapping>(registrations.Select(x => x.Build()));
        }

        protected override void Init()
        {
            var interceptorProvider = new InterceptorProvider(interceptorMappings);

            var interceptorRegistrations =
                interceptorMappings
                .Where(x => x.AutoRegister && string.IsNullOrEmpty(x.InterceptorKey) && !Kernel.HasComponent(x.InterceptorType))
                .Select(x => x.InterceptorType).Distinct()
                .Select(x => Component.For(typeof (IInterceptor)).ImplementedBy(x).Forward(x))
                .Cast<IRegistration>()
                .ToArray();

            Kernel.Register(interceptorRegistrations);
            Kernel.ComponentModelBuilder.AddContributor(interceptorProvider);
        }
    }
}