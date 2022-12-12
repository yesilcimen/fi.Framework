using fi.Core.Ioc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace fi.Ioc
{
    public sealed class IoCGenerator
    {
        public class DoTNet
        {
            private static readonly Lazy<DoTNet> lazy = new(() => new DoTNet());
            public static DoTNet Current => lazy.Value;
            private DoTNet()
            {
            }

            private readonly IDictionary<Type, Action<Type, IServiceCollection>> GetLifeCycle =
               new Dictionary<Type, Action<Type, IServiceCollection>> {
                { typeof(ISingletonDependency), (implementationType, service) => implementationType.GetTypeInfo().ImplementedInterfaces.Where(m => m != typeof(ISingletonDependency) && !m.Namespace.Equals("fi.Framework") && !m.Namespace.Contains("System")).ToList().ForEach(i => service.AddSingleton(i, implementationType)) },
                { typeof(IScopedDependency), (implementationType, service) => implementationType.GetTypeInfo().ImplementedInterfaces.Where(m => m != typeof(IScopedDependency) && !m.Namespace.Equals("fi.Framework") && !m.Namespace.Contains("System")).ToList().ForEach(i => service.AddScoped(i, implementationType)) },
                { typeof(ITransientDependency), (implementationType, service) => implementationType.GetTypeInfo().ImplementedInterfaces.Where(m => m != typeof(ITransientDependency) && !m.Namespace.Equals("fi.Framework") && !m.Namespace.Contains("System")).ToList().ForEach(i => service.AddTransient(i, implementationType)) },
                { typeof(IScopedSelfDependency), (implementationType, service) => service.AddScoped(implementationType) },
                { typeof(ISingletonSelfDependency), (implementationType, service) => service.AddSingleton(implementationType) },
                { typeof(ITransientSelfDependency), (implementationType, service) => service.AddTransient(implementationType) }
                };

            internal IServiceCollection Services { get; private set; }
            internal IConfiguration Configuration { get; private set; }

            /// <summary>
            /// .Net in standart IOC  kullanımı için geliştirilmiştir.
            /// </summary>
            /// <param name="services"></param>
            /// <param name="configuration"></param>
            public void Start(IServiceCollection services, IConfiguration configuration)
            {
                Services = services;
                Configuration = configuration;
                RegisterIntefaceBasedTypes();
                ConfigureOptionsCore();
            }
            /// <summary>
            /// .Net in standart Worker ları için geliştirilmiştir.
            /// </summary>
            /// <param name="hostContext"></param>
            /// <param name="services"></param>
            public void Start(HostBuilderContext hostContext, IServiceCollection services)
            {
                Services = services;
                Configuration = hostContext.Configuration;
                RegisterIntefaceBasedTypes();
                ConfigureOptionsCore();
                RegisterHostingWorker();
            }

            /// <summary>
            /// ITransientDependency,IScopedDependency veya ISingletonDependency nesneleri konteynıra ekliyor.
            /// </summary>
            private void RegisterIntefaceBasedTypes()
            {
                var dependencyObjects = AppDomain.Current.GetAllAssemblies().SelectMany(s => s.DefinedTypes.Where(w => !w.IsAbstract && w.IsClass).Select(sm => sm));

                var pureObjects = dependencyObjects.Where(w => (typeof(ITransientDependency).IsAssignableFrom(w)
                                            || typeof(ISingletonDependency).IsAssignableFrom(w)
                                            || typeof(IScopedDependency).IsAssignableFrom(w)
                                            || typeof(ISingletonSelfDependency).IsAssignableFrom(w)
                                            || typeof(ITransientSelfDependency).IsAssignableFrom(w)
                                            || typeof(IScopedSelfDependency).IsAssignableFrom(w))
                                        && !w.IsInterface
                                        && !w.IsAbstract)
                                  .Select(x => new TypeOfLifeCycle(x));

                if (!pureObjects.Any())
                    return;

                foreach (var dependencyObject in pureObjects)
                {
                    if (GetLifeCycle.TryGetValue(dependencyObject.LifeCycle, out Action<Type, IServiceCollection> _method))
                        _method.Invoke(dependencyObject.ImplementationType, Services);
                    else
                        throw new ArgumentNullException($"LifeCycle : {dependencyObject.LifeCycle.FullName} \r\n ImplementationType: {dependencyObject.ImplementationType.FullName}", "Ioc yükleme esnasında hata oluşmuştur.");
                }
            }
            /// <summary>
            /// IConfigurationOptions interface impelement eden option class'ları tüm projede aranıp Configuration yapısına inject ediliyoır.
            /// </summary>
            private void ConfigureOptionsCore()
            {
                var optionTypes = new List<Type>();
                foreach (var item in AppDomain.Current.GetAllAssemblies())
                {
                    optionTypes.AddRange(item.ExportedTypes);
                }
                optionTypes = optionTypes.Where(x => !x.IsAbstract && x.IsClass && typeof(IConfigurationOptions).IsAssignableFrom(x)).ToList();
                foreach (var options in optionTypes)
                {
                    typeof(OptionsConfigurationServiceCollectionExtensions).GetMethods().First()
                        .MakeGenericMethod(options)
                        .Invoke(Configuration, new object[] { Services, Configuration.GetSection(options.Name) });
                }
                foreach (var type in optionTypes)
                {
                    Services.AddSingleton(type, resolver =>
                    {
                        var optionType = typeof(IOptions<>).MakeGenericType(type);
                        return resolver.GetService(optionType).GetValue(nameof(IOptions<dynamic>.Value));
                    });
                }
            }
            /// <summary>
            /// IHostedService interface implement eden tüm nesneleri hostservice olarak IOC ye inject ediyor.
            /// </summary>
            private void RegisterHostingWorker()
            {
                MethodInfo methodInfo =
             typeof(ServiceCollectionHostedServiceExtensions)
             .GetMethods()
             .FirstOrDefault(p => p.Name == nameof(ServiceCollectionHostedServiceExtensions.AddHostedService));

                if (methodInfo == null)
                    throw new Exception($"'{nameof(IServiceCollection)}' içindeki '{nameof(ServiceCollectionHostedServiceExtensions.AddHostedService)}' extension methodu bulunamamıştır.");

                IEnumerable<Type> hostedServices_FromAssemblies = AppDomain.Current.GetAllAssemblies().SelectMany(a => a.DefinedTypes).Where(x => !x.IsAbstract && x.IsClass && x.GetInterfaces().Contains(typeof(IHostedService))).Select(p => p.AsType());

                foreach (Type hostedService in hostedServices_FromAssemblies)
                {
                    if (typeof(IHostedService).IsAssignableFrom(hostedService))
                    {
                        var genericMethod_AddHostedService = methodInfo.MakeGenericMethod(hostedService);
                        _ = genericMethod_AddHostedService.Invoke(obj: null, parameters: new object[] { Services });
                    }
                }
            }
        }
    }
}
