using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Karenia.TegamiHato.Server.Models;
using Karenia.TegamiHato.Server.Services;
using System.Threading.Tasks;
using System;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace Karenia.TegamiHato.Server.Controllers
{
    [ApiController]
    [Route("api/channel")]
    public class ChannelController : ControllerBase
    {
        public ChannelController(DatabaseService _db)
        {
            this.db = _db;
        }

        private DatabaseService db;

        public class AddChannelRequest
        {
            public string? channelName { get; set; } = null;
            public string channelTitle { get; set; } = "";
            public bool isPublic { get; set; } = false;
        }

        [HttpPost]
        [Authorize("api")]
        public async Task<IActionResult> AddChannel(
            [FromBody] AddChannelRequest req)
        {
            Ulid userId;
            {
                var id = HttpContext.User.Claims.Where(claim => claim.Type == "sub")
                    .Select(claim => claim.Value)
                    .Single();

                if (Ulid.TryParse(id, out var res)) userId = res;
                else return BadRequest(new ErrorResult(
                        "Bad userId", "Malformed bearer token. Please authorize again!"));
            }

            if (req.channelName != null && await db.ChannelNameExists(req.channelName))
            {
                return BadRequest(
                    new ErrorResult(
                        "Channel already exists", "The name of this channel is already taken"));
            }

            return Ok(
                await this.db.NewMailingChannel(
                    req.channelName, req.isPublic, req.channelTitle, userId));
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
                return Ok(this.db.GetRecentChannels(_id, count, skip));
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
            public List<string> tags { get; set; }
        }

        [HttpPost]
        [Route("message")]
        [Authorize("api")]
        public async Task<IActionResult> SendMessage(
            [FromRoute] string id,
            [FromBody] ApiSendMessage apiMessage
        )
        {
            if (!Ulid.TryParse(id, out var _id)) return BadRequest();
            var userId = Ulid.Parse(HttpContext.User.Claims.Where(claim => claim.Type == "sub")
                   .Select(claim => claim.Value)
                   .Single());
            User userInfo = (await this._db.GetUser(userId))!;

            var attachments = await this._db.GetAttachmentsFromIdAsync
                (apiMessage.attachments.Select(id => Ulid.Parse(id))
                .ToList());

            Ulid msgId = Ulid.NewUlid();
            var message = new HatoMessage()
            {
                MsgId = msgId,
                ChannelId = _id,
                Title = apiMessage.title,
                BodyHtml = apiMessage.bodyHtml,
                BodyPlain = apiMessage.bodyPlain,
                Attachments = attachments,
                SenderEmail = userInfo.Email,
                SenderNickname = userInfo.Nickname,
                Tags = apiMessage.tags,
            };
            await this._db.SaveMessageIntoChannel(message, _id);

            // HACK: Attachment are inserted directly!
            // TODO: Change it into a single call
            this._db.db.AttachmentRelations.AddRange(attachments.Select(att => new AttachmentMessageRelation()
            {
                AttachmentId = att.AttachmentId,
                MsgId = msgId
            }));
            await this._db.db.SaveChangesAsync();

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
        public async Task<IActionResult> GetRecentMessageAsync(
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
                    IList<HatoMessage> value = await _db.GetMessageFromChannelAsync(_id, _startId, count, ascending);
                    return base.Ok(value);
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
