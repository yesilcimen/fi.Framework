using System;
using System.Collections.Generic;

namespace fi.EFCore
{
    public abstract class AuditLogProvider
    {
        /// <summary>
        /// If you want use just a interceptors without logs.
        /// </summary>
        public AuditLogProvider()
        {
            Console.WriteLine($"AuditLogProvider: {GetType().GetHashCode()}");

            OnStart();
        }
        /// <summary>
        /// If you want to a IAuditable. Use this constructer. And generate a IAuditContext
        /// </summary>
        /// <param name="context"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public AuditLogProvider(IAuditContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            Console.WriteLine($"AuditLogProvider: {GetType().GetHashCode()}");
            Context = context;
            OnStart();
        }

        private void OnStart()
        {
            Interceptors = new Dictionary<Type, Func<IInterceptorGenerator>>
            {
                { typeof(IDeleteable), () => new DeleteableInterceptor() },
                { typeof(IDateable), () => new DateableInterceptor() },
                { typeof(ITraceable), () => new TraceableInterceptor(GetUserId) },
            };
        }

        private readonly IAuditContext Context;

        internal AuditableInterceptor AuditableInterceptor { get; private set; }
        public IDictionary<Type, Func<IInterceptorGenerator>> Interceptors { get; private set; }
        public abstract object GetUserId();
        internal void NewAuditableInterceptor() => AuditableInterceptor ??= new(Context, GetUserId);
        internal void AuditableInterceptorClear() => AuditableInterceptor = null;
    }

    internal class DefaultAuditLogProvider : AuditLogProvider
    {

        private static readonly Lazy<DefaultAuditLogProvider> lazy = new(() => new DefaultAuditLogProvider(), true);
        public static DefaultAuditLogProvider Current => lazy.Value;

        public override object GetUserId()
        {
            throw new NotImplementedException();
        }
    }
}
