using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Venus.Authentication.User;
using Venus.Core;

namespace Venus.Authentication
{
    [ApiController]
    [Authorize]
    [ServiceFilter(typeof(UserContextFilter))]
    [Route("api/[controller]/[action]")]
    public class VenusBaseController : Controller
    {
    }
}



