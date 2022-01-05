using Microsoft.EntityFrameworkCore;

namespace fi.EFCore
{
    public class EntityAudit
    {
        public EntityState State { get; internal set; }
        public string AuditMessage { get; internal set; }
        public string PrimaryKeyValue { get; internal set; }
        public string EntityName { get; internal set; }
    }
}
