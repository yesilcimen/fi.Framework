using fi.Core.Ioc;
using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace fi.Common
{
    public class TextJsonConverter : ISingletonSelfDependency
    {
        private readonly JsonSerializerOptions options = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        };

        public T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, options);
        public object Deserialize(string json, Type t) => JsonSerializer.Deserialize(json, t, options);
        public string Serialize<T>(T data) => JsonSerializer.Serialize(data, options);
    }
}
