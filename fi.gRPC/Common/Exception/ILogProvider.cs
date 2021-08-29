using fi.Core.Ioc;
using System.Threading.Tasks;

namespace fi.gRPC.Common
{
    public interface ILogProvider : ISingletonDependency
    {
        public Task WriteLogAsync(WebServiceLog data);
    }
}
