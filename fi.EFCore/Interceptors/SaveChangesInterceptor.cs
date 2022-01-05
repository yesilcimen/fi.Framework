using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace fi.EFCore
{
    public class SaveChangesInterceptor : ISaveChangesInterceptor, IDisposable
    {
        private EntityEntry[] entityEntries;
        public AuditLogProvider AuditLogProvider { get; }
        public IDictionary<int, IDictionary<Type, IInterceptorGenerator>> ContuniesInterceptor;
        public SaveChangesInterceptor(AuditLogProvider auditLogProvider)
        {
            Console.WriteLine($"SaveChangesInterceptor : {this.GetHashCode()}");
            if (auditLogProvider is not null)
                AuditLogProvider = auditLogProvider;
            ContuniesInterceptor = new Dictionary<int, IDictionary<Type, IInterceptorGenerator>>();
        }

        public void SaveChangesFailed(DbContextErrorEventData eventData)
        {
            InterceptError(eventData.Exception);
        }

        public Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
        {
            InterceptError(eventData.Exception);
            return Task.CompletedTask;
        }

        public int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            InterceptAfter();
            return result;
        }

        public async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            InterceptAfter();
            return await Task.FromResult(result);
        }

        public InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            entityEntries = eventData.Context.ChangeTracker.Entries().Where(w => w.State != EntityState.Detached && w.State != EntityState.Unchanged).ToArray();
            InterceptBefore(eventData.Context);

            foreach (var entry in entityEntries.Where(w => w.State is EntityState.Deleted))
                entry.State = EntityState.Modified;

            return result;
        }

        public async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                entityEntries = eventData.Context.ChangeTracker.Entries().Where(w => w.State != EntityState.Detached && w.State != EntityState.Unchanged).ToArray();
                InterceptBefore(eventData.Context);
                foreach (var entry in entityEntries.Where(w => w.State is EntityState.Deleted))
                    entry.State = EntityState.Modified;
            }
            return await Task.FromResult(result);
        }


        void InterceptBefore(DbContext context)
        {
            if (entityEntries.Any(entry => entry.Entity.GetType().GetInterfaces().Any(intface => intface.Equals(typeof(IAuditable)))))
                AuditLogProvider.NewAuditableInterceptor();

            foreach (var entry in entityEntries)
            {
                var interceptorGenerators = new Dictionary<Type, IInterceptorGenerator>();

                foreach (Type intface in entry.Entity.GetType().GetInterfaces().Where(w => !w.Equals(typeof(IAuditable)) && w.GetInterfaces().Any(a => a == typeof(IInterceptor))))
                {
                    if (AuditLogProvider.Interceptors.TryGetValue(intface, out Func<IInterceptorGenerator> p))
                    {
                        var generator = p();
                        interceptorGenerators.Add(intface, generator);
                        generator.OnBefore(entry, context);
                    }
                }

                if (entry.Entity.GetType().GetInterfaces().Any(a => a.Equals(typeof(IAuditable))))
                    AuditLogProvider.AuditableInterceptor.OnBefore(entry, context);

                ContuniesInterceptor.Remove(entry.Entity.GetHashCode());
                ContuniesInterceptor.Add(entry.Entity.GetHashCode(), interceptorGenerators);
            }
        }
        void InterceptAfter()
        {
            foreach (var entry in entityEntries)
            {
                foreach (Type intface in entry.Entity.GetType().GetInterfaces().Where(w => !w.Equals(typeof(IAuditable)) && w.GetInterfaces().Any(a => a == typeof(IInterceptor))))
                {
                    if (ContuniesInterceptor.TryGetValue(entry.Entity.GetHashCode(), out IDictionary<Type, IInterceptorGenerator> generators))
                        if (generators.TryGetValue(intface, out IInterceptorGenerator interceptorGenerator))
                            interceptorGenerator.OnAfter();
                }
            }

            if (entityEntries.Any(entry => entry.Entity.GetType().GetInterfaces().Any(intface => intface.Equals(typeof(IAuditable)))))
                AuditLogProvider.AuditableInterceptor?.OnAfter();

            AuditLogProvider.AuditableInterceptorClear();
        }
        void InterceptError(Exception exception)
        {
            foreach (var entry in entityEntries)
            {
                foreach (Type intface in entry.Entity.GetType().GetInterfaces().Where(w => !w.Equals(typeof(IAuditable)) && w.GetInterfaces().Any(a => a == typeof(IInterceptor))))
                {
                    if (ContuniesInterceptor.TryGetValue(entry.Entity.GetHashCode(), out IDictionary<Type, IInterceptorGenerator> generators))
                        if (generators.TryGetValue(intface, out IInterceptorGenerator interceptorGenerator))
                            interceptorGenerator.OnError(exception);
                }
            }
            if (entityEntries.Any(entry => entry.Entity.GetType().GetInterfaces().Any(intface => intface.Equals(typeof(IAuditable)))))
                AuditLogProvider.AuditableInterceptor?.OnError(exception);

            AuditLogProvider.AuditableInterceptorClear();
        }

        public void Dispose()
        {
            entityEntries = null;
            ContuniesInterceptor = null;
            GC.SuppressFinalize(this);
        }
        ~SaveChangesInterceptor() => Dispose();
    }
}
