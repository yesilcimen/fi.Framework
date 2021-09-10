using System;

namespace fi.Common
{
    public static class GuidExtension
    {
        public static bool IsEmpty(this Guid target) => target == Guid.Empty;
        public static bool IsNotEmpty(this Guid target) => !target.IsEmpty();
        public static bool IsNullOrEmpty(this Guid? target) => (target is null || target == Guid.Empty);
        public static bool IsNotNullOrEmpty(this Guid? target) => !target.IsNullOrEmpty();
    }
}
