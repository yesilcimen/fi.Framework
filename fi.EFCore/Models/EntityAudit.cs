using Microsoft.EntityFrameworkCore;

namespace fi.EFCore
{
    public class EntityAudit
    {
        public int Id { get; set; }
        public EntityState State { get; set; }
        public string AuditMessage { get; set; }

        public SaveChangesAudit SaveChangesAudit { get; set; }
    }
}
