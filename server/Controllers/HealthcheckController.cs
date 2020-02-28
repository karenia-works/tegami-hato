using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Karenia.TegamiHato.Server.Controllers
{
    [ApiController]
    [Route("/api/ping")]
    public class HealthCheckController : ControllerBase
    {
        [HttpGet]
        [HttpPost]
        public IActionResult Ping()
        {
            return NoContent();
        }
    }
}
