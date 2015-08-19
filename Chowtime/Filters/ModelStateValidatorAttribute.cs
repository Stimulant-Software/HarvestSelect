using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace SGApp.Filters
{
    /// <summary>
    ///  Filter attribute is used to validate model state on each request
    /// </summary>
    public class ModelStateValidatorAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext.ModelState.IsValid == false)
            {
                //TODO : Add more descriptive message. 
                actionContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid Model State");
            }
            
        }
    }
}