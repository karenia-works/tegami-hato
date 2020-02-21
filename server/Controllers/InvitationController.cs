using Microsoft.AspNetCore.Mvc;
using Karenia.TegamiHato.Server.Models;
using Karenia.TegamiHato.Server.Services;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Karenia.TegamiHato.Server.Controllers
{
    [ApiController]
    [Route("api/invitation")]
    public class InvitationController : ControllerBase
    {
        private readonly DatabaseService db;
        private readonly ILogger<InvitationController> logger;

        public InvitationController(
            DatabaseService db,
            ILogger<InvitationController> logger)
        {
            this.db = db;
            this.logger = logger;
        }

        [Authorize("api")]
        [HttpPost]
        [Route("{linkId}")]
        public async Task<IActionResult> Proceed(string linkId)
        {
            var _userId = HttpContext.User.Claims.Where(claim => claim.Type == "sub")
               .Select(claim => claim.Value)
               .Single();
            if (!Ulid.TryParse(_userId, out var userId))
            {
                return BadRequest(new ErrorResult(
                    "not deserialized", $"'{_userId}' is not a valid Ulid"));
            }


            var link = await db.GetInvitationLink(linkId);
            if (link != null)
            {
                var result = await db.AddUserToChannel(
                    userId,
                    link.ChannelId,
                    link.DefaultPermission,
                    true);

                // TODO: return the user with basic information about the channel added
                return result switch
                {
                    AddResult.Success => NoContent(),
                    AddResult.AlreadyExist => Conflict(
                        new ErrorResult(
                            "User already in channel"
                        )
                    ),
                    _ => BadRequest(new ErrorResult("Unable to add user to channel"))
                };
            }
            else
            {
                return BadRequest(new ErrorResult("No such invitation"));
            }
        }

        [HttpGet]
        [Route("{linkId}")]
        public async Task<IActionResult> GetInvitationLink(string linkId)
        {
            // TODO: report the user with basic information about the link
            throw new NotImplementedException();
        }
    }
}
