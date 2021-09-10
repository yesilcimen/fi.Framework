namespace fi.Common
{
    public static class BooleanExtension
    {
        public static bool IsNullOrEmpty(this bool? target) => target is null;
        public static bool IsNotNullOrEmpty(this bool? target) => target is not null;
    }
}
