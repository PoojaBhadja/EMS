using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
    }

    [Authorize]
    public class AutorizeController : BaseController
    {

    }

    [AllowAnonymous]
    public class AllowAnonymousController : BaseController
    {

    }
}
