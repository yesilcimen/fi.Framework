using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;

namespace fi.EFCore
{
    public abstract class InterceptorGenerator<T> : IInterceptorGenerator, IDisposable where T : IInterceptor
    {
        #region Overrides of Interceptor

        public void OnBefore(EntityEntry item, DbContext dbContext)
        {
            T tItem = (T)item.Entity;
            switch (item.State)
            {
                case EntityState.Added:
                    OnBeforeInsert(tItem, item, dbContext);
                    break;
                case EntityState.Deleted:
                    OnBeforeDelete(tItem, item, dbContext);
                    break;
                case EntityState.Modified:
                    OnBeforeUpdate(tItem, item, dbContext);
                    break;
            }
        }

        public void OnAfter() => OnAfterInsert();

        public void OnError(Exception exception) => OnAfterError(exception.Message);

        #endregion Overrides of Interceptor

        #region Before
        public abstract void OnBeforeInsert(T item, EntityEntry entityEntry, DbContext dbContext);

        public abstract void OnBeforeUpdate(T item, EntityEntry entityEntry, DbContext dbContext);

        public abstract void OnBeforeDelete(T item, EntityEntry entityEntry, DbContext dbContext);
        #endregion

        public abstract void OnAfterInsert();
        public abstract void OnAfterError(string execptionMessage);

        public void Dispose() => GC.SuppressFinalize(this);
    }
}
