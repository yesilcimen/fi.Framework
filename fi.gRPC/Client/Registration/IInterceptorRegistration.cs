using Microsoft.Extensions.DependencyInjection;

namespace fi.gRPC.Client
{
    public interface IInterceptorRegistration
    {
        void InstallInterceptors(IHttpClientBuilder builder);
    }
}
