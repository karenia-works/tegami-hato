using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Minio;
using NUlid;

namespace Karenia.TegamiHato.Server.Services
{
    public class ObjectStorageService
    {
        private readonly MinioClient client;
        private readonly string baseDomain;
        private readonly string spaceName;

        public ObjectStorageService(
            string baseDomain,
            string apiKey,
            string secretKey,
            string spaceName)
        {
            this.client = new MinioClient(baseDomain, apiKey, secretKey).WithSSL();
            this.baseDomain = baseDomain;
            this.spaceName = spaceName;
        }

        private readonly Regex FilenameRegex = new Regex("^(?<filename>.+)\\.(?<extension>\\w+)$");

        public async Task<string> PutAttachment(Ulid id, string filename, Stream data, long size, string contentType)
        {
            string key;
            var filenameMatchResult = FilenameRegex.Match(filename);
            if (filenameMatchResult.Success)
            {
                key = string.Format(
                    "{0}_{1}.{2}",
                    filenameMatchResult.Groups["filename"].Value,
                    id,
                    filenameMatchResult.Groups["extension"].Value);
            }
            else
            {
                key = filename + "_" + id.ToString();
            }
            return await PutObject(key, data, size, contentType);
        }
        public async Task<string> PutObject(string key, Stream data, long size, string contentType)
        {
            await this.client.PutObjectAsync(spaceName, key, data, size, contentType);
            return $"{spaceName}.{baseDomain}/{key}";
        }
    }
}
