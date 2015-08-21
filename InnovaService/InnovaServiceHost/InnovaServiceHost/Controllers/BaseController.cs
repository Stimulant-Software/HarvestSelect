using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using InnovaService;

namespace InnovaServiceHost.Controllers {
    public class BaseController : ApiController {
        #region Internal Members

        internal string _validationKey = "";

        #endregion

        #region Internal Methods

        internal static object ReturnPackage(object retVal) {
            var settings = new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore};
            var result = JsonConvert.SerializeObject(retVal, Formatting.None, settings);
            return result;
        }

        internal HttpResponseMessage returnPackage(HttpRequestMessage request, List<proc_sizes> obj){
            return request.CreateResponse(HttpStatusCode.OK, obj);
        }

        #endregion
    }
}
