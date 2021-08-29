using System;

namespace fi.gRPC.Common
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class GrpcExceptionSerializerAttribute : Attribute
    {
        public Type SerializerType { get; }

        public GrpcExceptionSerializerAttribute(Type serializerType) => SerializerType = serializerType;
    }
}
