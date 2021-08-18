using fi.Core;
using fi.Core.Ioc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace fi.API
{
    /// <summary>
    /// MonitoringActionFilter gelen requestleri izlememizi sağlatır.
    /// Konteynır içerisine Scope olarak eklenmiştir.
    /// </summary>
    /// 
    public class MonitoringFilterAction : IActionFilter, IOrderedFilter, IScopedSelfDependency
    {
        private readonly IMonitorLogManager logManager;
        public MonitoringFilterAction(IMonitorLogManager _logManager)
        {
            logManager = _logManager;
        }

        public int Order { get; set; }
        public virtual void OnActionExecuting(ActionExecutingContext context)
        {
            var description = ((ControllerActionDescriptor)context.ActionDescriptor);

            bool hasServiceLog = description.MethodInfo.CustomAttributes.Any(item => item.AttributeType == typeof(ServiceLogAttribute));

            if (logManager.TryCreate(description.ActionName, context.ActionArguments, hasServiceLog, out WebServiceLog SessionInformation))
            {
                context.HttpContext.Items.Add("SessionInformation", SessionInformation);
                context.HttpContext.Items.Add("HasServiceLog", hasServiceLog.ToString());
            }

        }

        public virtual void OnActionExecuted(ActionExecutedContext context)
        {
            logManager.CompleteLog(context, context.HttpContext.Items);
        }

        public void Dispose()
        {
            System.GC.SuppressFinalize(this);
        }
    }
}
