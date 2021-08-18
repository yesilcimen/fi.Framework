using System;

namespace fi.Core
{
    /// <summary>
    /// Request ve Response datalarının loglanması isteniyorsa eklkenmesi yeterli
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ServiceLogAttribute : Attribute
    {
    }
}
