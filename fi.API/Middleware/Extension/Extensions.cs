using Microsoft.AspNetCore.Builder;

namespace fi.API
{
    public static class Extensions
    {
        public static IApplicationBuilder UseEtaExceptionMiddleware(this IApplicationBuilder builder) => builder.UseMiddleware<ExceptionMiddleware>();
    }
}
