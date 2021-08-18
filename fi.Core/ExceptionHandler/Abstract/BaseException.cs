using System;
using System.Collections.Generic;
using System.Linq;

namespace fi.Core
{
    public abstract class BaseException : Exception
    {
        public ErrorModel ErrorModel { get; private set; }

        /// <summary>
        /// Custom exceptionlar icin kullanilicak ctor'dur.
        /// </summary>
        /// <param name="errorResults"></param>
        /// <param name="statusCode">Api icin HttpStatusCode gRPC icin StatusCode kullanalim</param>
        /// <param name="responseMessageType"></param>
        public BaseException(ICollection<ErrorResult> errorResults, int statusCode, ResponseMessageType responseMessageType) : base(string.Join(Environment.NewLine, errorResults.Select(x => x.ToString())))
        {
            ErrorModel = new ErrorModel(errorResults, statusCode, responseMessageType);
        }
    }
}
