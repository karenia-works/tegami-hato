using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Karenia.TegamiHato.Server.Models;
using Karenia.TegamiHato.Server.Services;
using System.Threading.Tasks;
using NUlid;
using System;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using IdentityServer4;
using System.Linq;

namespace Karenia.TegamiHato.Server.Controllers
{
    [ApiController]
    [Route("api/channel")]
    public class ChannelController : ControllerBase
    {
        public ChannelController(DatabaseService _db)
        {
            this._db = _db;
        }

        private DatabaseService _db;

        public class AddChannelRequest
        {
            public string? channelName { get; set; } = null;
            public string channelTitle { get; set; } = "";
            public bool isPublic { get; set; } = false;
        }

        [HttpPost]
        public async Task<IActionResult> AddChannel(
            [FromBody] AddChannelRequest req)
        {
            if (req.channelName != null && await _db.ChannelNameExists(req.channelName))
            {
                return BadRequest(
                    new ErrorResult(
                        "Channel already exists", "The name of this channel is already taken"));
            }
            else
            {
                return Ok(
                    await this._db.NewMailingChannel(
                        req.channelName, req.isPublic, req.channelTitle));
            }
        }

        [HttpGet]
        [Route("recent")]
        [Authorize("api")]
        public IActionResult GetRecentChannelEntries(
            [FromQuery] int count = 20,
            [FromQuery] int skip = 0
        )
        {
            Ulid _id;
            {
                var id = HttpContext.User.Claims.Where(claim => claim.Type == "sub")
                    .Select(claim => claim.Value)
                    .Single();

                if (Ulid.TryParse(id, out var res)) _id = res; else return BadRequest();
            }

            try
            {
                return Ok(this._db.GetRecentChannels(_id, count, skip));
            }
            catch (ArgumentOutOfRangeException e)
            {
                return BadRequest(e);
            }
        }
    }

    [ApiController]
    [Route("api/channel/{id:Regex(^\\w{{26}}$)}")]
    public class ChannelIdController : ControllerBase
    {
        public ChannelIdController(
            DatabaseService _db,
            EmailRecvAdaptor recvAdaptor)
        {
            this._db = _db;
            this.recvAdaptor = recvAdaptor;
        }

        private readonly DatabaseService _db;
        private readonly EmailRecvAdaptor recvAdaptor;

        [HttpGet]
        public async Task<IActionResult> GetChannel([FromRoute] string id)
        {
            // throws exception on error
            Ulid? _id;
            { if (Ulid.TryParse(id, out var res)) _id = res; else _id = null; }
            var name = id;

            if (_id != null)
            {
                var res = await this._db.GetChannel(_id.Value);
                if (res != null) return Ok(res);
                else return NotFound();
            }
            else
            {
                var res = await this._db.GetChannelFromUsername(name);
                if (res != null) return Ok(res);
                else return NotFound();
            }
        }

        [HttpPost]
        [Authorize("api")]
        [Route("join")]
        public async Task<IActionResult> JoinChannel(
            [FromRoute] string id
        // [FromQuery] string userId
        )
        {
            var userId = HttpContext.User.Claims.Where(claim => claim.Type == "sub")
                .Select(claim => claim.Value)
                .Single();
            if (Ulid.TryParse(id, out var _channelId))
            {
                if (Ulid.TryParse(userId, out var _userId))
                {
                    var result = await this._db.AddUserToChannel(_userId, _channelId);
                    return result switch
                    {
                        AddResult.Success => NoContent(),
                        AddResult.AlreadyExist => BadRequest(new ErrorResult(
                            "resource already exists",
                            "The user has already been in the channel")),
                        _ => BadRequest(),
                    };
                }
                else
                {
                    return BadRequest(new ErrorResult(
                        "not deserialized", $"'{userId}' is not a valid Ulid"));
                }
            }
            else
            {
                return BadRequest(new ErrorResult(
                    "not deserialized", $"'{id}' is not a valid Ulid"));
            }
        }

        public class SendMessageResult
        {
            [JsonConverter(typeof(UlidJsonConverter))]
            public Ulid MsgId { get; set; }
            public DateTimeOffset timestamp { get; set; }
            public List<(string, string)> FailedChannels { get; set; }
        }

        public class ApiSendMessage
        {
            public string? title { get; set; }

            public string bodyPlain { get; set; }

            public string? bodyHtml { get; set; }

            public List<string> attachments { get; set; }

        }

        [HttpPost]
        [Route("message")]
        public async Task<IActionResult> SendMessage(
            [FromRoute] string id,
            [FromBody] HatoMessage message
        )
        {
            if (!Ulid.TryParse(id, out var _id)) return BadRequest();
            var msgId = await this._db.SaveMessageIntoChannel(message, _id);

            // Send message to channel
            var channelEmails = await recvAdaptor.GetEmailsFromChannelIds(new[] { _id });
            var failed = await recvAdaptor.SendEmail(message, channelEmails);

            return Ok(new SendMessageResult()
            {
                MsgId = msgId,
                timestamp = msgId.Time,
                FailedChannels = failed
            });
        }


        [HttpGet]
        [Route("message")]
        public IActionResult GetRecentMessage(
            [FromRoute] string id,
             string startId = "7fffffffffffffffffffffffff",
             int count = 20,
             bool ascending = false
        )
        {
            if (Ulid.TryParse(id, out var _id))
            {
                if (Ulid.TryParse(startId, out var _startId))
                {
                    return Ok(this._db.GetMessageFromChannel(_id, _startId, count, ascending));
                }
                else
                {
                    return BadRequest(new ErrorResult(
                        "not deserialized", $"'{startId}' is not a valid Ulid"));
                }
            }
            else
            {
                return BadRequest(new ErrorResult(
                    "not deserialized", $"'{id}' is not a valid Ulid"));
            }
        }
    }
}
