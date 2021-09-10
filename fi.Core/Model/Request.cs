using System;
using System.Collections.Generic;
using System.Linq;

namespace fi.Core
{
    public interface IValidate
    {
        bool IsValid { get; }
        string UniqueValue { get; set; }
        ICollection<ErrorResult> ValidateResults { get; }
    }

    public interface IValidator : IValidate
    {
        void Validate();
    }

    public abstract class RequestValidate : IRequest, IValidator
    {
        public RequestValidate()
        {
            ValidateResults = new HashSet<ErrorResult>();
        }
        public Guid UserId { get; set; }
        public Guid? PersonId { get; set; }
        public Guid? AccountId { get; set; }
        [DoNotSerialize]
        public bool IsValid => !ValidateResults.Any();
        [DoNotSerialize]
        public string UniqueValue { get; set; }
        /// <summary>
        /// ValidateResults nesnesi : <see cref = "ValidationResult"  /> tipiyle validasyon nesnenizi ekleyin.
        /// </summary>
        [DoNotSerialize]
        public ICollection<ErrorResult> ValidateResults { get; }
        /// <summary>
        /// Validate result nesnesinin içerisine validasyonlarınızı ekleyiniz.
        ///  <see cref = "ValidateResults" />
        /// </summary>
        public abstract void Validate();
    }

    public interface IRequest
    {
        Guid UserId { get; set; }
    }
}
