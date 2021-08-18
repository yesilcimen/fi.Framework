using System;

namespace fi.Core.Ioc
{
    /// <summary>
    /// Konteynera Scope durumunda eklemek için kullanılır. Kalıtım alınan interface in altına eklenir.
    /// Örnek: IOrderService : <see cref="IScopedDependency"/> sonrasında OrderService : IOrderService gibi kullanılmalıdır.
    /// </summary>
    public interface IScopedDependency : IDisposable { }

    /// <summary>
    /// Konteynera Scope durumunda eklemek için kullanılır. Direk sınıfa interface olarak verilebilir. 
    /// Örnek : OrderService : <see cref="IScopedSelfDependency"/> gibi kullanılabilir.
    /// </summary>
    public interface IScopedSelfDependency : IDisposable { }
}
