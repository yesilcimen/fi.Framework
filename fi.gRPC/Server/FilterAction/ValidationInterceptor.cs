using fi.Core;
using fi.Core.Ioc;
using fi.gRPC.Common;
using Grpc.Core;
using Grpc.Core.Interceptors;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace fi.gRPC.Server
{
    public class ValidationInterceptor : Interceptor, IScopedSelfDependency
    {
        private readonly IMonitorLogServer _logManager;
        public ValidationInterceptor(IMonitorLogServer logManager) => _logManager = logManager;

        public void Dispose() => GC.SuppressFinalize(this);

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request,
                                                                                      ServerCallContext context,
                                                                                      UnaryServerMethod<TRequest, TResponse> continuation)
        {
            await ValidateRequestAsync(request, context);
            return await continuation(request, context);
        }


        public override async Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request,
                                                                                     IServerStreamWriter<TResponse> responseStream,
                                                                                     ServerCallContext context,
                                                                                     ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            await ValidateRequestAsync(request, context);
            await continuation(request, responseStream, context);
        }


        public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream,
                                                                                     ServerCallContext context,
                                                                                     ClientStreamingServerMethod<TRequest, TResponse> continuation)
        {
            var asyncStreamReader = new TracingAsyncStreamReader<TRequest>(requestStream, request => ValidateRequestAsync(request, context));
            return await continuation(asyncStreamReader, context);
        }

        public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream,
                                                                                     IServerStreamWriter<TResponse> responseStream,
                                                                                     ServerCallContext context,
                                                                                     DuplexStreamingServerMethod<TRequest, TResponse> continuation)
        {
            var asyncStreamReader = new TracingAsyncStreamReader<TRequest>(requestStream, request => ValidateRequestAsync(request, context));
            await continuation(asyncStreamReader, responseStream, context);
        }




        #region private methods

        private Task ValidateRequestAsync<TRequest>(TRequest request, ServerCallContext context) where TRequest : class
        {
            if (request is RequestValidate requestValidate)
            {
                requestValidate.Validate();

                if (requestValidate.ValidateResults.Count > 0)
                {
                    var customGrpcException = new GrpcException(requestValidate.ValidateResults, 3, ResponseMessageType.ValidationException);

                    if (GrpcExceptionHandler.SetException(context.ResponseTrailers, customGrpcException))
                    {
                        if (_logManager.TryServerSideAddResponseLog(customGrpcException.ErrorModel, context))
                        {
                            _logManager.CompleteServerSideLog(context);

                            throw new RpcException(new Status(StatusCode.InvalidArgument, Constants.GrpcMetaDataKeyError), new Metadata());
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }

        #endregion private methods
    }
}
