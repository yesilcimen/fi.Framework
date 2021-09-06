using System;
using System.Collections.Generic;

namespace fi.EFCore
{
    public abstract class AuditLogProvider
    {
        public AuditLogProvider(IAuditContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            Console.WriteLine($"AuditLogProvider: {GetType().GetHashCode()}");

            Context = context;
            Interceptors = new Dictionary<Type, Func<IInterceptorGenerator>>
            {
                { typeof(IDeleteable), () => new DeleteableInterceptor() },
                { typeof(IDateable), () => new DateableInterceptor() },
                { typeof(ITraceable), () => new TraceableInterceptor(GetUserId()) },
                { typeof(IAuditable), () => new AuditableInterceptor(context) }
            };
        }
        internal IAuditContext Context { get; }
        public IDictionary<Type, Func<IInterceptorGenerator>> Interceptors { get; }
        public abstract Guid GetUserId();
    }
}
