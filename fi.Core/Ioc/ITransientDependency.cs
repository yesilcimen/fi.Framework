using System;

namespace fi.Core.Ioc
{
    /// <summary>
    /// Konteynera Transient durumunda eklemek için kullanılır. Direk sınıfa interface olarak verilebilir. 
    /// Örnek : OrderService : <see cref="ITransientDependency"/> gibi kullanılabilir.
    /// </summary>
    public interface ITransientDependency : IDisposable { }

    /// <summary>
    /// Konteynera Transient durumunda eklemek için kullanılır. Direk sınıfa interface olarak verilebilir. 
    /// Örnek : OrderService : <see cref="ITransientSelfDependency"/> gibi kullanılabilir.
    /// </summary>
    public interface ITransientSelfDependency : IDisposable { }
}
