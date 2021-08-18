using fi.Core;
using System.Collections.Generic;

namespace fi.API
{
    internal class RequestException : BaseException
    {
        internal RequestException(ICollection<ErrorResult> errorResults, int code, ResponseMessageType responseMessageType) : base(errorResults, code, responseMessageType)
        {
        }
    }
}
