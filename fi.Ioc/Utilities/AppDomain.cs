using fi.Core.Ioc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;


namespace fi.Ioc
{
    internal class AppDomain
    {
        private static readonly Lazy<AppDomain> lazy = new(() => new AppDomain());
        public static AppDomain Current => lazy.Value;
        private readonly ICollection<Assembly> assemblies;
        private AppDomain() { assemblies = new HashSet<Assembly>(); }

        public ICollection<Assembly> GetAllAssemblies()
        {
            if (assemblies.Any())
                return assemblies;

            var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            var files = Directory.GetFiles(path, "*.dll").Where(i => !new string[] { "Microsoft.", "System." }.Any(j => i.Contains(j)));

            foreach (string item in files)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(item);
                    assemblies.Add(assembly);
                }
                catch { }
            }

            return assemblies;


            #region Yontem 1: Current projede ilgili dll kullanilmissa ekler.
            //if (assemblies != null)
            //    return assemblies;
            //assemblies = Assembly.GetEntryAssembly().GetReferencedAssemblies().Select(Assembly.Load).ToList();
            //assemblies.Add(Assembly.GetEntryAssembly());
            //return assemblies;
            #endregion Yontem 1: Current projede ilgili dll kullanilmissa ekler.


            #region Yontem 2: tum assembly'de recursive gezmek. Not asagidaki kod recursive degildir sadece bir alta inmisdir.
            //if (assemblies != null)
            //    return assemblies;

            //assemblies = new List<Assembly> { Assembly.GetEntryAssembly() };

            //Assembly.GetEntryAssembly().GetReferencedAssemblies().Select(Assembly.Load).ToList().ForEach(assembly =>
            //{
            //    if (!assemblies.Any(u => u.FullName == assembly.FullName))
            //        assemblies.Add(assembly);

            //    assembly.GetReferencedAssemblies().Select(Assembly.Load).ToList().ForEach(innerAssembly =>
            //    {
            //        if (!assemblies.Any(u => u.FullName == innerAssembly.FullName))
            //            assemblies.Add(innerAssembly);
            //    });
            //});

            //return assemblies;
            #endregion Yontem 2: tum assembly'de recursive gezmek. Not yukaridaki kod recursive degildir sadece bir alta inmisdir.
        }
    }

    internal class TypeOfLifeCycle
    {
        public TypeOfLifeCycle(Type implementationType)
        {
            ImplementationType = implementationType;
        }
        public Type ImplementationType { get; }
        public Type LifeCycle => typeof(ITransientDependency).IsAssignableFrom(ImplementationType) ? typeof(ITransientDependency) : typeof(IScopedDependency).IsAssignableFrom(ImplementationType) ? typeof(IScopedDependency) : typeof(ISingletonDependency).IsAssignableFrom(ImplementationType) ? typeof(ISingletonDependency) : typeof(IScopedSelfDependency).IsAssignableFrom(ImplementationType) ? typeof(IScopedSelfDependency) : typeof(ISingletonSelfDependency).IsAssignableFrom(ImplementationType) ? typeof(ISingletonSelfDependency) : typeof(ITransientSelfDependency).IsAssignableFrom(ImplementationType) ? typeof(ITransientSelfDependency) : null;
    }

    internal static class ObjectExtensions
    {
        internal static object GetValue(this object obj, string propertyNameToGetValueFrom) => obj.GetType().GetProperty(propertyNameToGetValueFrom)?.GetValue(obj, null);
    }
}
