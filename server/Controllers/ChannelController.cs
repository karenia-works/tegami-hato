using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Karenia.TegamiHato.Server.Models;
using Karenia.TegamiHato.Server.Services;
using System.Threading.Tasks;
using NUlid;

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
        public async Task<IActionResult> GetChannel(
            [FromQuery] string? name = null,
            [FromQuery] string? id = null)
        {
            // throws exception on error
            Ulid? _id;
            {
                if (id != null)
                    if (Ulid.TryParse(id, out var res)) _id = res;
                    else return BadRequest($"'{id}' is not a valid Ulid");
                else _id = null;
            }

            if (_id != null)
            {
                var res = await this._db.GetChannel(_id.Value);
                if (res != null)
                    if (name != null && res.ChannelUsername == name) return Ok(res);
                if (name == null) return Ok(res);
                else return NotFound();
            }
            else if (name != null)
            {
                var res = await this._db.GetChannelFromUsername(name);
                if (res != null) return Ok(res);
                else return NotFound();
            }
            else return BadRequest(new ErrorResult("Either name or id should be set"));
        }
    }
}
