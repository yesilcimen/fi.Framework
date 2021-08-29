using fi.Core.Ioc;
using Grpc.Core;

namespace fi.gRPC.Common
{
    public interface IMonitorLogClient : ITransientDependency
    {
        bool TryClientSideAddResponseLog<TResponse>(TResponse response, WebServiceLog log) where TResponse : class;
        void CompleteClientSideLog(WebServiceLog log);
        bool TryCreateLog<TRequest>(string actionName, MethodType methodType, TRequest request, bool hasServiceLog, out WebServiceLog log) where TRequest : class;
    }
}
