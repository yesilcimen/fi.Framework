using System.ComponentModel.DataAnnotations;

namespace fi.EFCore
{
    public abstract class Entity : IEntity
    {
    }
    public abstract class Entity<T> : IEntity<T>
    {
        public Entity(T id) => Id = id;

        [Key]
        public T Id { get; private set; }
    }
}
