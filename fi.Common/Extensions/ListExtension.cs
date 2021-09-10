using System.Collections.Generic;
using System.Linq;

namespace fi.Common
{
    public static class ListExtension
    {
        public static bool IsNull<T>(this T obj) where T : class, new() => obj is null;
        public static bool IsNotNull<T>(this T obj) where T : class, new() => !obj.IsNull();
        public static bool IsNullOrEmpty<T>(this List<T> list) => list is null || !list.Any();
        public static bool IsNotNullOrEmpty<T>(this List<T> list) => !list.IsNullOrEmpty();
    }
}
