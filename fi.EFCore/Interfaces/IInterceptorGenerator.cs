using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;

namespace fi.EFCore
{
    public interface IInterceptorGenerator
    {
        void OnBefore(EntityEntry item, DbContext objectContext);
        void OnAfter();
        void OnError(Exception exception);
    }
}
