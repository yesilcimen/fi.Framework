using fi.Core;
using fi.Core.Ioc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace fi.API
{
    public interface IMonitorLogManager : IScopedDependency
    {
        bool TryCreate(string actionName, IDictionary<string, object> arguments, bool hasServiceLog, out WebServiceLog log);

        void CompleteLog(ActionExecutedContext context, IDictionary<object, object> contextItems);
    }

    public class MonitorLogManager : IMonitorLogManager
    {

        public bool TryCreate(string actionName, IDictionary<string, object> arguments, bool hasServiceLog, out WebServiceLog log)
        {
            log = new WebServiceLog
            {
                MonitorLog = new MonitorLog
                {
                    Id = Guid.NewGuid(),
                    ActionName = actionName,
                    ActiveDate = DateTime.Now
                },
            };

            if (hasServiceLog)
            {
                string requestText = JsonSerializer.Serialize(arguments);

                log.ServiceLog = new ServiceLog
                {
                    Id = Guid.NewGuid(),
                    MonitorLogId = log.MonitorLog.Id,
                    Request = requestText,
                    RequestTime = DateTime.Now,
                };
            }

            return true;
        }
        public void CompleteLog(ActionExecutedContext context, IDictionary<object, object> contextItems)
        {
            if (bool.TryParse(contextItems["HasServiceLog"]?.ToString(), out bool hasServiceLog))
            {
                if (contextItems["SessionInformation"] is WebServiceLog sessionInformation)
                {
                    string responseText = "";
                    if (hasServiceLog)
                    {
                        var options = new JsonSerializerOptions
                        {
                            WriteIndented = true,
                            PropertyNameCaseInsensitive = true,
                            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
                        };
                        responseText = context.Exception switch
                        {
                            BaseException => JsonSerializer.Serialize(((BaseException)context.Exception).ErrorModel, options),
                            _ => JsonSerializer.Serialize((context.Result as ObjectResult).Value, options)
                        };

                        sessionInformation.ServiceLog.Response = responseText;
                        sessionInformation.ServiceLog.ResponseTime = DateTime.Now;
                        sessionInformation.ServiceLog.Duration = (long?)DateTime.Now.Subtract(sessionInformation.ServiceLog.RequestTime).TotalMilliseconds;
                    }

                    Console.WriteLine($"Request : {sessionInformation?.ServiceLog?.Request } {Environment.NewLine} Response : {responseText}");
                }
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }

    public class MonitorLog
    {
        public Guid Id { get; set; }
        public string ActionName { get; set; }
        public DateTime ActiveDate { get; set; }
    }

    public class ServiceLog
    {
        public Guid Id { get; set; }
        public object MonitorLogId { get; set; }
        public string Request { get; set; }
        public DateTime RequestTime { get; set; }
        public string Response { get; set; }
        public DateTime ResponseTime { get; set; }
        public long? Duration { get; set; }
    }

    public class WebServiceLog
    {
        public MonitorLog MonitorLog { get; set; }
        public ServiceLog ServiceLog { get; internal set; }
    }
}
