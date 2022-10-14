using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace OpenTelemetryDemo.Controllers
{
    [RoutePrefix("api/values")]
    public class ValuesController : ApiController
    {
        // GET api/values
        [Route]
        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            await Task.Delay(200);
            using (Activity activity = Telemetry.ActivitySource.StartActivity("SomeActivity"))
            {
                activity.SetTag("mytag", "tag-value");
                await Task.Delay(200);
            }
            return new [] { "value1", "value2" };
        }
    }
}
