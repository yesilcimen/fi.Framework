using Microsoft.AspNetCore.Mvc;

namespace fi.API
{
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [ServiceFilter(typeof(MonitoringFilterAction))]
    [ServiceFilter(typeof(ValidationFilterAction))]
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {

    }
}
