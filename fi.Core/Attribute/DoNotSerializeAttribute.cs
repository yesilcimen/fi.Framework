using System;

namespace fi.Core
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class DoNotSerializeAttribute : Attribute
    {
    }
}
