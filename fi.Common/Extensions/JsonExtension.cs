using System.Text.Json;

namespace fi.Common
{
    public static class JsonExtension
    {
        public static T FromJson<T>(this string value) => JsonSerializer.Deserialize<T>(value);
        public static string ToJson(this object data) => JsonSerializer.Serialize(data);
    }
}
