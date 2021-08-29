using System;

namespace fi.gRPC.Common
{
    public class MonitorLog
    {
        public Guid Id { get; set; }
        public string ActionName { get; set; }
        public DateTime ActiveDate { get; set; }
        public string MethodType { get; set; }
    }
}
