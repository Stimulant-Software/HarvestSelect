using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SGApp.DTOs;
using System.Web;

namespace SGApp.Controllers {
    public class JMremoteController : ApiController {
        [HttpPost]
        public object GetRemoteData([FromBody] SGApp.DTOs.GenericDTO dto) {
            var startDate = dto.startDate.Split('-');
            var endDate = dto.endDate.Split('-');
            var yr = int.Parse(startDate[0]);
            var mon = int.Parse(startDate[1]);
            var day = int.Parse(startDate[2]);
            dto.StartDate = new DateTime(yr, mon, day);
            yr = int.Parse(endDate[0]);
            mon = int.Parse(endDate[1]);
            day = int.Parse(endDate[2]);
            dto.EndDate= new DateTime(yr, mon, day);
            var client = new HttpClient {
                //BaseAddress = new Uri("http://323-booth-svr2:3030/")
                BaseAddress = new Uri("http://localhost:51888/")
                //BaseAddress = new Uri(baseAddress)                
            };
            try {
                var response = client.PostAsJsonAsync("api/Remote/GetKeithsData", dto).Result;
                response.EnsureSuccessStatusCode();
                var result = response.Content.ReadAsStringAsync().Result;
                return result;               
            }
            catch (Exception e) {
                throw new HttpException("Error occurred: " + e.Message);
            }
        }
    }
}
