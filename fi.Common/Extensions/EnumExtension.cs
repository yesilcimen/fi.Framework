using System;
using System.Globalization;
using System.Reflection;

namespace fi.Common
{
    public static class EnumExtension
    {
        public static T GetAttributeFromMember<T>(this Enum @enum) where T : Attribute
        {
            T result = default;
            MemberInfo[] member = @enum.GetType().GetMember(@enum.ToString());

            if (member.Length == 1)
            {
                object[] customAttributes = member[0].GetCustomAttributes(typeof(T), false);
                if (customAttributes[0] is T)
                {
                    result = (T)((object)customAttributes[0]);
                }
            }

            return result;
        }
        public static T Parse<T>(this Enum  _, string value)=> (T)Enum.Parse(typeof(T), value);
        public static string ToClassName(this Enum style, string prefix)
        {
            var styleText = style.ToString().ToDashCase().ToLower(CultureInfo.InvariantCulture);
            return string.Concat(prefix, styleText);
        }
        public static bool Contains(this Enum @enum, Enum flag)
        {
            if (@enum.GetType() != flag.GetType())
                throw new ArgumentException($"Enum tipleri aynı olmalıdır. Enum : {@enum.GetType()} Flag : {flag.GetType()}");

            var keysVal = Convert.ToUInt64(@enum);
            var flagVal = Convert.ToUInt64(flag);

            return (keysVal & flagVal) == flagVal;
        }

    }
}
