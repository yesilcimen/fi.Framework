using fi.Core.Ioc;
using Grpc.Core;

namespace fi.gRPC.Common
{
    public interface IMonitorLogServer : IScopedDependency
    {
        bool TryServerSideAddResponseLog<TResponse>(TResponse response, ServerCallContext contextItems) where TResponse : class;
        void CompleteServerSideLog(ServerCallContext contextItems);
        bool TryCreateLog<TRequest>(string actionName, MethodType methodType, TRequest request, bool hasServiceLog, out WebServiceLog log) where TRequest : class;
    }
}
