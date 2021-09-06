using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace fi.EFCore
{
    internal class DeleteableInterceptor : InterceptorGenerator<IDeleteable>
    {
        public override void OnAfterError(string execptionMessage) { }
        public override void OnAfterInsert() { }
        public override void OnBeforeDelete(IDeleteable item, EntityEntry entityEntry, DbContext dbContext) => item.AuditIsDeleted = true;
        public override void OnBeforeInsert(IDeleteable item, EntityEntry entityEntry, DbContext dbContext) => item.AuditIsDeleted = false;
        public override void OnBeforeUpdate(IDeleteable item, EntityEntry entityEntry, DbContext dbContext) { }
    }
}
