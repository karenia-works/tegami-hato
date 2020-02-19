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
            var result = await db.GenerateLoginCodeOrAddUser(email, code);

            this.logger.LogInformation($"Added login code '{code.Code}' for {email}");
            if (result.Item1) this.logger.LogInformation($"Added user");

            return Ok(new
            {
                UserId = result.Item2,
                NewUser = result.Item1
            });
        }
    }
}
