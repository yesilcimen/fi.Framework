namespace fi.Core.Ioc
{
    /// <summary>
    /// Konteynera Singleton durumunda eklemek için kullanılır. Direk sınıfa interface olarak verilebilir. 
    /// Örnek : OrderService : <see cref="ISingletonDependency"/> gibi kullanılabilir.
    /// </summary>
    public interface ISingletonDependency { }

    /// <summary>
    /// Konteynera Singleton durumunda eklemek için kullanılır. Direk sınıfa interface olarak verilebilir. 
    /// Örnek : OrderService : <see cref="ISingletonSelfDependency"/> gibi kullanılabilir.
    /// </summary>
    public interface ISingletonSelfDependency { }
}
