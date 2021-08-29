using Grpc.AspNetCore.Server;

namespace fi.gRPC.Server
{
    public interface IInterceptorRegistration
    {
        void InstallInterceptors(GrpcServiceOptions options);
    }
}
