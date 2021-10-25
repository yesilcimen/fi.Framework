using Microsoft.AspNetCore.Builder;

namespace fi.API
{
    public static class Extensions
    {
        public static IApplicationBuilder UseFiExceptionMiddleware(this IApplicationBuilder builder) => builder.UseMiddleware<ExceptionMiddleware>();
    }
}
