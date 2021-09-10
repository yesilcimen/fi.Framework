using System;
using System.Globalization;
using System.Linq;

namespace fi.Common
{
    public static class ObjectExtension
    {
        public static bool IsNumeric(this object @object, NumberStyles numberStyle = NumberStyles.Any) => Double.TryParse(@object?.ToString() ?? string.Empty, numberStyle, NumberFormatInfo.CurrentInfo, out double _);

        public static bool Between(this IComparable current, IComparable first, IComparable second) => current.CompareTo(first) >= 0 && current.CompareTo(second) <= 0;

        public static bool TryGetValue<T>(this object source, string propertyName, out T result)
        {
            result = default;

            if (source is null)
                return false;

            var val = source.GetType().GetProperties().Where(w => w.Name.Equals(propertyName)).Select(s => s.GetValue(source)).FirstOrDefault();

            if (val is null)
                return false;

            if (!(val is T))
                return false;

            try
            {
                result = (T)Convert.ChangeType(val, typeof(T));
            }
            catch (InvalidCastException)
            {
                return false;
            }

            return true;
        }

        public static void SetValue(this object source, string propertyName, object value)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source), "Parametre boş geçilemez.");

            if (propertyName is null)
                throw new ArgumentNullException(nameof(propertyName), "Parametre boş geçilemez.");

            try
            {
                source.GetType().GetProperty(propertyName).SetValue(source, value);
            }
            catch (Exception ex)
            {
                throw new InvalidCastException(ex.Message);
            }
        }
    }
}
