using System;
using System.Threading;
using System.Threading.Tasks;

namespace fi.EFCore
{
    public interface IAuditContext
    {
        void Commit();
        void Add(SaveChangesAudit audit);
        Task CommitAsync(CancellationToken cancellationToken);
    }
}
