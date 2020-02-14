using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Karenia.TegamiHato.Server.Models;
using Karenia.TegamiHato.Server.Services;
using System.Threading.Tasks;
using NUlid;
using System;
using System.Text.Json.Serialization;

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
        [Route("{id}")]
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
        [Route("{id}/join")]
        public async Task<IActionResult> JoinChannel(
            [FromRoute] string id,
            [FromQuery] string userId
        )
        {
            if (Ulid.TryParse(id, out var _channelId))
            {
                if (Ulid.TryParse(userId, out var _userId))
                {
                    var result = await this._db.AddUserToChannel(_userId, _channelId);
                    switch (result)
                    {
                        case AddResult.Success:
                            return NoContent();
                        case AddResult.AlreadyExist:
                            return BadRequest(new ErrorResult(
                                "resource already exists",
                                "The user has already been in the channel"
                            ));
                        default:
                            return BadRequest();
                    }
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
        }

        [HttpPost]
        [Route("{id}/message")]
        public async Task<IActionResult> SendMessage(
            [FromRoute] string id,
            [FromBody] HatoMessage message
        )
        {
            if (!Ulid.TryParse(id, out var _id)) return BadRequest();
            var msgId = await this._db.SaveMessageIntoChannel(message, _id);
            return Ok(new SendMessageResult()
            {
                MsgId = msgId,
                timestamp = msgId.Time
            });
        }


        [HttpGet]
        [Route("{id}/message")]
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
