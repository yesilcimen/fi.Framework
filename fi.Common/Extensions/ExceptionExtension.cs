using System;
using System.Text;

namespace fi.Common
{
    public static class ExceptionExtension
    {
        public static string CreateExceptionString(this Exception exception)
        {
            var sb = new StringBuilder();
            CreateExceptionString(sb, exception, string.Empty);
            return sb.ToString();
        }
        private static void CreateExceptionString(StringBuilder sb, Exception e, string indent)
        {
            if (indent is null)
                indent = string.Empty;
            else if (indent.Length > 0)
                sb.Append($"{indent}Inner ");

            sb.Append($"Exception Found:{Environment.NewLine}{indent}Type: {e.GetType().FullName}" +
                $"{Environment.NewLine}{indent}Message: {e.Message}" +
                $"{Environment.NewLine}{indent}Source: {e.Source}" +
                $"{Environment.NewLine}{indent}Stacktrace: {e.StackTrace}");

            if (e.InnerException is not null)
            {
                sb.Append('\n');
                CreateExceptionString(sb, e.InnerException, indent + "  ");
            }
        }
    }
}
