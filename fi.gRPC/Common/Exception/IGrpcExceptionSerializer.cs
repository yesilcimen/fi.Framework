using System;
using System.IO;

namespace fi.gRPC.Common
{
    public interface IGrpcExceptionSerializer
    {
        void Serialize(Exception exception, Stream stream);
        Exception Deserialize(Stream stream);
    }
}
