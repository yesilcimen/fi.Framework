using System;

namespace fi.EFCore
{
    public interface ITraceable : IInterceptor
    {
        Guid AuditCreateBy { get; set; }
        Guid AuditModifiedBy { get; set; }
    }

}
