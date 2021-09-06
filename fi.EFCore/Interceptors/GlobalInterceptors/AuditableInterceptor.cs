using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Linq;

namespace fi.EFCore
{
    internal class AuditableInterceptor : InterceptorGenerator<IAuditable>
    {
        private readonly IAuditContext _auditContext;
        private SaveChangesAudit audit;

        public AuditableInterceptor(IAuditContext auditContext)
        {
            _auditContext = auditContext;
        }

        public override void OnAfterInsert()
        {
            audit.Succeeded = true;
            audit.EndTime = DateTime.Now;
            _auditContext.Add(audit);
            _auditContext.Commit();
        }
        public override void OnBeforeDelete(IAuditable item, EntityEntry entityEntry, DbContext dbContext)
        {
            audit = new() { AuditId = Guid.NewGuid(), StartTime = DateTime.Now };
            audit.Entities.Add(new EntityAudit { State = EntityState.Deleted, AuditMessage = CreateDeletedMessage(entityEntry) });

            static string CreateDeletedMessage(EntityEntry entry)
                => entry.Properties.Where(property => property.Metadata.IsPrimaryKey()).Aggregate(
                    $"Deleting {entry.Metadata.DisplayName()} with ",
                    (auditString, property) => auditString + $"{property.Metadata.Name}: '{property.CurrentValue}' ");
        }
        public override void OnBeforeInsert(IAuditable item, EntityEntry entityEntry, DbContext dbContext)
        {
            audit = new() { AuditId = Guid.NewGuid(), StartTime = DateTime.Now };
            audit.Entities.Add(new EntityAudit { State = EntityState.Added, AuditMessage = CreateAddedMessage(entityEntry) });

            static string CreateAddedMessage(EntityEntry entry)
               => entry.Properties.Aggregate(
                   $"Inserting {entry.Metadata.DisplayName()} with ",
                   (auditString, property) => auditString + $"{property.Metadata.Name}: '{property.CurrentValue}' ");
        }
        public override void OnBeforeUpdate(IAuditable item, EntityEntry entityEntry, DbContext dbContext)
        {
            audit = new() { AuditId = Guid.NewGuid(), StartTime = DateTime.Now };
            audit.Entities.Add(new EntityAudit { State = EntityState.Modified, AuditMessage = CreateModifiedMessage(entityEntry) });

            static string CreateModifiedMessage(EntityEntry entry)
                => entry.Properties.Where(property => property.IsModified || property.Metadata.IsPrimaryKey()).Aggregate(
                    $"Updating {entry.Metadata.DisplayName()} with ",
                    (auditString, property) => auditString + $"{property.Metadata.Name}: '{property.CurrentValue}' ");
        }
        public override void OnAfterError(string execptionMessage)
        {
            audit.Succeeded = false;
            audit.EndTime = DateTime.Now;
            audit.ErrorMessage = execptionMessage;
            _auditContext.Add(audit);
            _auditContext.Commit();
        }
    }
}
