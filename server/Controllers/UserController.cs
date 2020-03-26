using System;
using System.Threading.Tasks;
using FluentEmail.Core;
using Karenia.TegamiHato.Server.Models;
using Karenia.TegamiHato.Server.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace Karenia.TegamiHato.Server.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        public UserController(
            DatabaseService db,
            EmailSendingService send,
            Microsoft.AspNetCore.Hosting.IWebHostEnvironment env,
            ILogger<UserController> logger)
        {
            this.db = db;
            this.send = send;
            this.env = env;
            this.logger = logger;
        }

        private DatabaseService db;
        private readonly EmailSendingService send;
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment env;
        private readonly ILogger<UserController> logger;

        [HttpPost]
        [Route("code/request")]
        public async Task<IActionResult> RequestLoginCode(string email)
        {
            var code = UserLoginCode.Generate(DateTimeOffset.Now);
            var user = await db.GetUserFromEmail(email);
            var emailResult = await SendLoginCodeAsync(
                email,
                user?.Nickname ?? email,
                code.Code);

            if (!emailResult.Successful)
                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    new ErrorResult("Email service unavailable"));

            var result = await db.GenerateLoginCodeOrAddUser(email, code);

            if (env.IsDevelopment())
                logger.LogInformation($"Added login code '{code.Code}' for {email}");

            if (result.Item1) this.logger.LogInformation($"Added user");

            return Ok(new
            {
                UserId = result.Item2,
                NewUser = result.Item1
            });
        }

        [HttpGet]
        [Route("me")]
        [Authorize("api")]
        public async Task<IActionResult> GetMe()
        {
            Ulid _id;
            {
                var id = HttpContext.User.Claims.Where(claim => claim.Type == "sub")
                    .Select(claim => claim.Value)
                    .Single();

                if (Ulid.TryParse(id, out var res)) _id = res; else return BadRequest();
            }

            var user = await this.db.GetUser(_id);
            return Ok(user);
        }

        [HttpGet]
        [Route("{id:Regex(^\\w{{26}}$)}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            Ulid _id;
            {
                if (Ulid.TryParse(id, out var res)) _id = res; else return BadRequest();
            }

            var user = await this.db.GetUser(_id);
            return Ok(user);
        }

        private async Task<FluentEmail.Core.Models.SendResponse> SendLoginCodeAsync(string receiver, string nickname, string code)
        {
            var email = new Email(string.Format(
                "code-noreply@{0}",
                send.Domain), "Hato Code Bot")
            .To(receiver)
            .Subject($"Your login code is \"{code}\"")
            .Body(Views.LoginCodeTemplate.compiledTemplate(new { nickname, code }), true)
            .PlaintextAlternativeBody(Views.LoginCodeTemplate.compiledTemplatePlaintext(new { nickname, code }))
            ;

            return await send.SendEmail(email);
        }
    }
}
