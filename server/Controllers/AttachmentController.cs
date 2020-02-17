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
using System.IO;

namespace Karenia.TegamiHato.Server.Controllers
{
    [Authorize("api")]
    [ApiController]
    [Route("api/attachment")]
    public class AttachmentController : ControllerBase
    {
        private readonly ObjectStorageService oss;
        private readonly DatabaseService db;

        public AttachmentController(ObjectStorageService oss, DatabaseService db)
        {
            this.oss = oss;
            this.db = db;
        }

        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> UploadAttachment(
            // [FromBody] Stream fileStream,
            [FromQuery] string filename,
            [FromHeader(Name = "Content-Length")] long contentLength,
            [FromHeader(Name = "Content-Type")] string? contentType)
        {
            var fileStream = Request.Body;
            if (contentType == null)
            {
                contentType = MimeTypes.GetMimeType(filename);
            }
            Ulid id = Ulid.NewUlid();
            var path = await this.oss.PutAttachment(id, filename, fileStream, contentLength, contentType);
            return Created(path, new
            {
                path,
                size = contentLength,
            });
        }
    }
}
