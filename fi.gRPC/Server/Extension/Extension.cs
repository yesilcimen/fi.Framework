using fi.Core;
using Grpc.AspNetCore.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace fi.gRPC.Server
{
    public static class Extension
    {
        public static ErrorModel ToValidationTrailers(this ICollection<ErrorResult> validateResults) => new ErrorModel(validateResults, 3, ResponseMessageType.ValidationException);


        /// <summary>
        /// install all server Interceptors
        /// </summary>
        /// <param name="options"></param>
        public static void RegistrServerInterceptors(this GrpcServiceOptions options)
        {
            Assembly.GetExecutingAssembly()
               .ExportedTypes
               .Where(x => typeof(IInterceptorRegistration).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
               .Select(Activator.CreateInstance)
               .Cast<IInterceptorRegistration>()
               .ToList().ForEach(interceptor => interceptor.InstallInterceptors(options));
        }
    }
}
