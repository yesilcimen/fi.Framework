using fi.Core;
using System.Collections.Generic;

namespace fi.gRPC.Common
{
    [GrpcExceptionSerializer(typeof(GrpcExceptionSerializer))]
    internal class GrpcException : BaseException
    {
        internal GrpcException(ICollection<ErrorResult> errorResults, int code, ResponseMessageType responseMessageType) : base(errorResults, code, responseMessageType)
        {
        }
    }
}
