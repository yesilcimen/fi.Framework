namespace fi.EFCore
{
    public interface IDeleteable : IInterceptor
    {
        bool AuditIsDeleted { get; set; }
    }

}
