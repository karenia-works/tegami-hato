using System;
using System.Threading.Tasks;
using Karenia.TegamiHato.Server.Models;
using Karenia.TegamiHato.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Karenia.TegamiHato.Server.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        public UserController(
            DatabaseService _db,
            ILogger<UserController> logger)
        {
            this.db = _db;
            this.logger = logger;
        }

        private DatabaseService db;
        private readonly ILogger<UserController> logger;

        [HttpPost]
        [Route("code/request")]
        public async Task<IActionResult> RequestLoginCode(string email)
        {
            // Whether the code is sent should not be exposed to user.
            var code = UserLoginCode.Generate(DateTimeOffset.Now);
            var result = await db.GenerateLoginCode(email, code);

            // TODO: Send login code to email!
            if (result) this.logger.LogInformation($"Added login code '{code.Code}' for {email}");
            else this.logger.LogInformation($"Failed to validate {email} in login code");

            return NoContent();
        }
    }
}
