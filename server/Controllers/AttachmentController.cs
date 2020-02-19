using Microsoft.AspNetCore.Mvc;
using Karenia.TegamiHato.Server.Models;
using Karenia.TegamiHato.Server.Services;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;

namespace Karenia.TegamiHato.Server.Controllers
{
    [Authorize("api")]
    [ApiController]
    [Route("api/attachment")]
    public class AttachmentController : ControllerBase
    {
        private readonly ObjectStorageService oss;
        private readonly DatabaseService db;
        private readonly EmailSendingService send;

        public AttachmentController(
            ObjectStorageService oss,
            DatabaseService db,
            EmailSendingService send)
        {
            this.oss = oss;
            this.db = db;
            this.send = send;
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

            var att = new HatoAttachment()
            {
                AttachmentId = id,
                Filename = filename,
                Url = path,
                ContentType = contentType,
                Size = contentLength,
                IsAvailable = true
            };
            await this.db.AddAttachmentEntry(att);

            return Created(path, att);
        }
    }
}
