using fi.Core.Ioc;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace fi.HangfireCommon;

public static class HangfireExtension
{
    public static IServiceCollection HangfireCommon(this IServiceCollection serviceCollection, string sqlConnectionString, SqlServerStorageOptions sqlServerStorageOptions)
        => serviceCollection
            .AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(sqlConnectionString, sqlServerStorageOptions))
            .AddHangfireServer();

    public static IApplicationBuilder UseHangfireCommon(this IApplicationBuilder applicationBuilder)
    {
        var abstractClassWorkers = AllAssemblies
            .SelectMany(sm => sm.ExportedTypes)
            .Where(t =>
                t.IsAbstract &&
                t.IsClass &&
                t.BaseType.Equals(typeof(AutoHangfireBase)))
            .Select(t => t);

        var hangfireCommonBaseList = AllAssemblies
            .SelectMany(sm => sm.ExportedTypes)
            .Where(t =>
                (!t.IsAbstract && t.IsClass && t.BaseType.Equals(typeof(AutoHangfireBase)))
                || (abstractClassWorkers.Contains(t.BaseType)))
            .Select(t => t);

        Type[] scops = { typeof(IScopedDependency), typeof(IScopedSelfDependency) };
        foreach (Type item in hangfireCommonBaseList)
        {
            AutoHangfireBase autoHangfireBase;

            if (item.GetInterfaces().Any(x => scops.Contains(x)))
            {
                using IServiceScope scoped = applicationBuilder.ApplicationServices.CreateScope();
                autoHangfireBase = scoped.ServiceProvider.GetRequiredService(item) as AutoHangfireBase;
            }
            else
                autoHangfireBase = applicationBuilder.ApplicationServices.GetRequiredService(item) as AutoHangfireBase;

            if (autoHangfireBase is null)
                throw new ArgumentNullException($"{item.FullName} is not null - {nameof(HangfireCommon)}");
            if (string.IsNullOrEmpty(autoHangfireBase.Name))
                throw new ArgumentNullException($"{item.FullName} Name is not null - {nameof(HangfireCommon)}");
            if (string.IsNullOrEmpty(autoHangfireBase.TimeCron))
                throw new ArgumentNullException($"{item.FullName} TimeCron is not null - {nameof(HangfireCommon)}");

            RecurringJob.AddOrUpdate(autoHangfireBase.Name, () => autoHangfireBase.RunJob(), autoHangfireBase.TimeCron);
        }

        return applicationBuilder;
    }

    private static ICollection<Assembly> AllAssemblies
    {
        get
        {
            string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string[] exludeAssemblies = new string[] { "Microsoft.", "System." };
            IEnumerable<string> files = Directory.GetFiles(path, "*.dll")
                                                 .Where(i => !exludeAssemblies.Any(j => i.Contains(j)));

            HashSet<Assembly> assemblies = new();
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
        }
    }

    public static void JobAddOrUpdate(this HangfireBase hangfire)
        => RecurringJob.AddOrUpdate(hangfire.Name, () => hangfire.RunJob(hangfire.Args), hangfire.TimeCron);
    public static void JobRemove(this HangfireBase hangfire)
        => RecurringJob.RemoveIfExists(hangfire.Name);
}
