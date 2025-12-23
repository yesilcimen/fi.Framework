namespace fi.HangfireCommon
{
    public abstract class AutoHangfireBase : HangfireBase { }
    public abstract class HangfireBase
    {
        public abstract string TimeCron { get; set; }
        public abstract string Name { get; set; }
        public abstract object[] Args { get; set; }

        public abstract void RunJob(params object[] args);
    }
}