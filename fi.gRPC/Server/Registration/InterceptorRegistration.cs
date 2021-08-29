using Grpc.AspNetCore.Server;

namespace fi.gRPC.Server
{
    public class InterceptorRegistration : IInterceptorRegistration
    {
        public void InstallInterceptors(GrpcServiceOptions options)
        {
            options.Interceptors.Add<MonitoringInterceptor>();
            options.Interceptors.Add<ValidationInterceptor>();
            options.Interceptors.Add<ExceptionInterceptor>();
        }
    }
}
