using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core;
using Castle.Core.Internal;
using Castle.DynamicProxy;
using Castle.MicroKernel;
using Castle.MicroKernel.ModelBuilder;
using Castle.MicroKernel.ModelBuilder.Descriptors;
using Castle.Windsor.Interceptors.Facility.Registration;
using Common.Logging;

namespace Castle.Windsor.Interceptors.Facility
{
    public class InterceptorProvider : IContributeComponentModelConstruction
    {
        private static readonly ILog Logger = LogManager.GetLogger<InterceptorProvider>();
        private readonly InterceptorSelectorDescriptor selectorDescriptor;

        public InterceptorProvider(IList<InterceptorMapping> interceptorMappings)
        {
            InterceptorMappings = interceptorMappings;

            var selector = new InterceptorSelector(InterceptorMappings);
            selectorDescriptor = new InterceptorSelectorDescriptor(new InstanceReference<IInterceptorSelector>(selector));
        }

        public IList<InterceptorMapping> InterceptorMappings { get; private set; }

        public void ProcessModel(IKernel kernel, ComponentModel model)
        {
            // we cannot assign an interceptor to an existing inteceptor, so ignore
            if (IsInterceptor(model))
            {
                return;
            }

            var interceptors = GetInterceptorsApplicableTo(model);
            if (interceptors.Count == 0) {
                return;
            }

            Logger.TraceFormat("Applying interceptors {0} to component {1}...",
                               string.Join(", ", interceptors.Select(x => x.InterceptorType.Name).ToArray()),
                               model.ComponentName);

            // add interceptors to the component model
            interceptors.ForEach(x => model.Interceptors.AddIfNotInCollection(GetInterceptorReference(x)));

            // add interceptor selector to the component model
            selectorDescriptor.BuildComponentModel(kernel, model);
        }

        private bool IsInterceptor(ComponentModel model)
        {
            return typeof (IInterceptor).IsAssignableFrom(model.Implementation);
        }

        protected InterceptorReference GetInterceptorReference(InterceptorMapping mapping)
        {
            return string.IsNullOrEmpty(mapping.InterceptorKey) ? 
                InterceptorReference.ForType(mapping.InterceptorType) : 
                InterceptorReference.ForKey(mapping.InterceptorKey);
        }

        protected ICollection<InterceptorMapping> GetInterceptorsApplicableTo(ComponentModel model)
        {
            var interceptors = new HashSet<InterceptorMapping>();

            var typesToScan = new List<Type>();
            typesToScan.Add(model.Implementation);
            typesToScan.AddRange(model.Services);

            foreach (var typeToScan in typesToScan) {
                var type = typeToScan;
                foreach (var interceptor in InterceptorMappings.Where(x => x.TypeSelector(type)))
                {
                    interceptors.Add(interceptor);
                }
            }
            return interceptors;
        }
    }
}