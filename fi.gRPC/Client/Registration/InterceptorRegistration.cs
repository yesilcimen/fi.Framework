using Microsoft.Extensions.DependencyInjection;

namespace fi.gRPC.Client
{
    public class InterceptorRegistration : IInterceptorRegistration
    {
        public void InstallInterceptors(IHttpClientBuilder builder) => builder.AddInterceptor<MonitoringInterceptor>();
    }
}
