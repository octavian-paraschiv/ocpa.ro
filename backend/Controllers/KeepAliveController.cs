using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace OPMedia.Backend.Controllers
{
    public class KeepAliveController : ApiController
    {
        [HttpGet]
        public string KeepAlive()
        {
            return "ok";
        }
    }

    public class IsLocalAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(HttpActionContext context)
        {
            if (!context.RequestContext.IsLocal)
                throw new UnauthorizedAccessException();
        }
    }

}