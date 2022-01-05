using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;

namespace fi.EFCore
{
    internal class TraceableInterceptor : InterceptorGenerator<ITraceable>
    {
        private readonly Guid userId;
        public TraceableInterceptor(Func<object> userId) => Guid.TryParse(userId()?.ToString(), out this.userId);
        public override void OnAfterError(string execptionMessage) { }
        public override void OnAfterInsert() { }
        public override void OnBeforeDelete(ITraceable item, EntityEntry entityEntry, DbContext dbContext) => item.AuditModifiedBy = userId;
        public override void OnBeforeInsert(ITraceable item, EntityEntry entityEntry, DbContext dbContext) => item.AuditCreateBy = item.AuditModifiedBy = userId;
        public override void OnBeforeUpdate(ITraceable item, EntityEntry entityEntry, DbContext dbContext) => item.AuditModifiedBy = userId;
    }
}
