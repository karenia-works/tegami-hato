using Microsoft.AspNetCore.Mvc;
using Karenia.TegamiHato.Server.Models;
using Karenia.TegamiHato.Server.Services;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

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

        public const long MaxAllowedUploadSize = (long)(2.5 * 1024 * 1024 * 1024);

        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> UploadAttachment(
            [FromQuery] string filename
        )
        {
            var _contentLength = Request.ContentLength;
            if (_contentLength == null)
                return StatusCode(StatusCodes.Status411LengthRequired);
            if (_contentLength > MaxAllowedUploadSize)
                return StatusCode(StatusCodes.Status413PayloadTooLarge);
            long contentLength = _contentLength.Value;

            var contentType = Request.ContentType;
            if (contentType == null)
            {
                contentType = MimeTypes.GetMimeType(filename);
            }

            var fileStream = Request.Body;
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
