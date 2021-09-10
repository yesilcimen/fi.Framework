using fi.Core;
using fi.Core.Ioc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace fi.API
{
    public class ValidationFilterAction : IActionFilter, IOrderedFilter, IScopedSelfDependency
    {
        private bool disposedValue;

        public int Order { get; set; }

        public virtual void OnActionExecuting(ActionExecutingContext context)
        {
            List<ErrorResult> etaRequestValidates = new();

            #region Validation
            foreach (var value in context.ActionArguments.Values)
            {
                #region UserId ve PersonId
                var validInfoArray = value as IRequest[] ?? new IRequest[] { value as IRequest }.Where(w => w is not null);

                foreach (var validInfo in validInfoArray)
                {
                    validInfo.UserId = System.Guid.Empty/*Login olan kullanıcının id si verilmelidir.*/;
                }
                #endregion

                var validArray = value as RequestValidate[] ?? new RequestValidate[] { value as RequestValidate }.Where(w => w is not null);

                foreach (var valid in validArray)
                {
                    valid.Validate();

                    if (valid.ValidateResults.Count > 0)
                        etaRequestValidates.AddRange(valid.ValidateResults);
                }

            }
            if (etaRequestValidates.Count > 0)
                throw new RequestException(etaRequestValidates, (int)HttpStatusCode.UnprocessableEntity, ResponseMessageType.ValidationException);

            #endregion
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {

        }

        #region Disposable pattern
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ValidationFilterAction()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }
        #endregion
    }
}
