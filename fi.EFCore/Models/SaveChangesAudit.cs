using System;
using System.Collections.Generic;

namespace fi.EFCore
{
    public class SaveChangesAudit
    {
        public Guid AuditId { get; internal set; }
        public DateTime StartTime { get; internal set; }
        public DateTime EndTime { get; internal set; }
        public bool Succeeded { get; internal set; }
        public string ErrorMessage { get; internal set; }
        public string AuditUserId { get; internal set; }

        public ICollection<EntityAudit> Entities { get; } = new HashSet<EntityAudit>();
    }
}
