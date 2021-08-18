using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace fi.Core
{
    public class ErrorModel
    {
        public ErrorModel(ICollection<ErrorResult> errorResults, int code, ResponseMessageType type)
        {
            Type = type.ToString();
            Code = code;
            ErrorResults = errorResults;
        }

        public string Type { get; set; }
        [JsonIgnore]
        public int Code { get; set; }
        public ICollection<ErrorResult> ErrorResults { get; set; }
        public string Message => ErrorResults.Count > 0 ? string.Join(Environment.NewLine, ErrorResults.Select(s => s.Message)) : null;
    }

    public class ErrorResult
    {
        public string Field { get; set; }

        public string Message { get; set; }

        public ErrorResult(string message, string field = null)
        {
            Message = message;
            Field = field;
        }
    }

    public enum ResponseMessageType
    {
        Info = 1,
        Warning = 2,
        Error = 3,
        UnhandledException = 4,
        ValidationException = 5,
        Question = 6
    }
}
