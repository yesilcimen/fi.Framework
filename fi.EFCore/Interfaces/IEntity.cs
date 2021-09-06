namespace fi.EFCore
{

    public interface IEntity
    {
    }
    public interface IEntity<T> : IEntity
    {
        T Id { get; }
    }

}
