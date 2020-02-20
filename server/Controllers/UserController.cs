using System;
using System.Threading.Tasks;
using FluentEmail.Core;
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
            DatabaseService db,
            EmailSendingService send,
            ILogger<UserController> logger)
        {
            this.db = db;
            this.send = send;
            this.logger = logger;
        }

        private DatabaseService db;
        private readonly EmailSendingService send;
        private readonly ILogger<UserController> logger;

        [HttpPost]
        [Route("code/request")]
        public async Task<IActionResult> RequestLoginCode(string email)
        {
            var code = UserLoginCode.Generate(DateTimeOffset.Now);
            var result = await db.GenerateLoginCodeOrAddUser(email, code);

            this.logger.LogInformation($"Added login code '{code.Code}' for {email}");
            await SendLoginCodeAsync(email, code.Code);
            if (result.Item1) this.logger.LogInformation($"Added user");

            return Ok(new
            {
                UserId = result.Item2,
                NewUser = result.Item1
            });
        }

        private async Task SendLoginCodeAsync(string receiver, string code)
        {
            var email = new Email(string.Format(
                "code-noreply@{0}",
                send.Domain), "Hato Code Bot")
            .To(receiver)
            .Subject($"Your login code is \"{code}\"")
            .Body(string.Format(@"
<h1> Login </h1>
<p>
    Hi, we've seen you trying to log in in just now. Here's your login code:
    <br/>
    <pre class=""login - code""> {0} </pre>
</p>
<p>
  If you haven't requested a login, please ignore or delete this email. The code expires in 15 minutes.
</p>
<sub>
  <b> Tegami Hato </b>: A Simple information broadcasting tool.
</sub>
<style>
body {{
  font-family: 'Source Han Sans CN', 'Sarasa UI SC', 'Noto Sans', 'San Fransisco', 'Segoe UI', 'Roboto, 'Noto Sans CJK SC', 'Noto Sans SC', 'Source Han Sans SC', 'Microsoft Yahei UI', sans-serif;
}}
.login-code{{
  font-family: 'Iosevka', 'IBM Plex Mono', 'Consolas', 'SF Mono', 'Roboto Mono', monospace;
  font-size: 2em;
  padding: 0.5em;
  text-align: center;
  background-color: #88888844;
  border-radius: 0.2em;
  max-width: 12em;
}}
</style>
            ", code), true)
            .PlaintextAlternativeBody(string.Format(@"
##   Login   ##

Hi, we've seen you trying to log in in just now. Here's your login code:

    >>>> {0} <<<<

If you haven't requested a login, please ignore or delete this email. The code expires in 15 minutes.

---
Tegami Hato: A Simple information broadcasting tool.
            ", code));

            await send.SendEmail(email);
        }
    }
}
