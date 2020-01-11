using System;
using FluentEmail.Mailgun;
using FluentEmail.Core;
using FluentEmail.Core.Models;
using FluentEmail;
using System.Net.Http;
using Microsoft.AspNetCore;
using System.Threading.Tasks;
using Karenia.TegamiHato.Server.Models;
using System.IO;
using System.Text.Json;
// using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Karenia.TegamiHato.Server.Services
{
    public class EmailSendingService
    {
        public EmailSendingService(string domain, string apiKey, ILogger<EmailSendingService> logger)
        {
            this.sender = new MailgunSender(domain, apiKey);
            this.domain = domain;
            this.logger = logger;
            // this.factory = new FluentEmailFactory(sender);
            logger.LogInformation("Started sending service for {0}", domain);
        }

        private MailgunSender sender;
        private string domain;
        private ILogger<EmailSendingService> logger;
        public string Domain { get => domain; }

        public async Task<SendResponse> SendEmail(EmailData data)
        {
            var email = new Email();
            email.Data = data;
            return await sender.SendAsync(email);
        }

        public async Task<SendResponse> SendEmail(IFluentEmail email)
        {
            return await sender.SendAsync(email);
        }
    }

    public class KebabNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            return Regex.Replace(name, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1-").ToLower();
        }
    }


    public class EmailRecvService
    {
        public EmailRecvService(string domain, string apiKey, ILogger<EmailRecvService> logger)
        {
            this.domain = domain;
            this.authParam =
                System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"api:{apiKey}"));
            this.client = new HttpClient();
            this.logger = logger;

            this.client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", this.authParam);
            Console.WriteLine("nya!");
            logger.LogInformation("Started receiving service for {0}", domain);
        }

        private ILogger<EmailRecvService> logger;
        private HttpClient client;

        public event Action<EmailRecvEvent> OnEmailRecv;

        private string domain;

        private string authParam;

        private TimeSpan firstRetrievalOffset = new TimeSpan(hours: 0, minutes: -30, seconds: 0);

        public async void beginEmailLoop()
        {
            var lastRetrieval = DateTime.Now + firstRetrievalOffset;
            while (true)
            {
                try
                {
                    lastRetrieval = await getEmails(lastRetrieval);
                }
                catch (Exception e) { logger.LogError(e, "Failed to get email"); }
                await Task.Delay(30_000);
            }
        }

        public async Task<DateTime> getEmails(DateTime lastRetrieval)
        {
            var begin = lastRetrieval.ToUniversalTime() - DateTime.UnixEpoch;
            var now = DateTime.Now.ToUniversalTime();
            var end = now - DateTime.UnixEpoch;
            logger.LogInformation("Pulling email from {0}, ranging from {1} to {2}", domain, DateTime.UnixEpoch + begin, now);
            var result = await client.GetAsync($"https://api.mailgun.net/v3/{domain}/events?event=stored&begin={begin.TotalSeconds}&end={end.TotalSeconds}");
            if (!result.IsSuccessStatusCode)
            {
                // TODO
                var res = await result.Content.ReadAsStringAsync();
                throw new Exception($"Failed to get events: {res}");
            }
            var resultStr = await result.Content.ReadAsStreamAsync();

            var resultObject = await JsonSerializer.DeserializeAsync<MailgunEventList<MailgunStorageEvent>>(resultStr, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = new KebabNamingPolicy(),
            });

            var latestMsg = lastRetrieval;

            logger.LogInformation("Email pulled, got {} items", resultObject.Items.Count);
            foreach (var item in resultObject.Items)
            {
                var thisMsgTime = DateTime.UnixEpoch.AddSeconds(item.Timestamp);
                if (thisMsgTime > latestMsg) latestMsg = thisMsgTime;
                getEmail(item.Storage.Url);
            }

            if (resultObject.Items.Count > 0)
                return now;
            else return lastRetrieval;
        }

        private async void getEmail(string url)
        {
            var result = await client.GetAsync(url);
            if (!result.IsSuccessStatusCode)
            {
                // TODO
                throw new Exception("Failed to get email");
            }
            var resultStr = await result.Content.ReadAsStreamAsync();
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = new KebabNamingPolicy(),
            };
            options.Converters.Add(new DateTimeSerializer());
            var resultObject = await JsonSerializer.DeserializeAsync<MailgunEmailRaw>(resultStr, options);

            logger.LogInformation("Got new email: {0}\nFrom {1}\nTo {2}", resultObject.Subject, resultObject.From, resultObject.To);

            this.OnEmailRecv?.Invoke(new EmailRecvEvent()
            {
                email = resultObject
            });
        }

        public async Task<Stream> GetAttachment(string url)
        {
            var req = await client.GetAsync(url);
            if (!req.IsSuccessStatusCode) { throw new AttachmentNotFoundException(); }
            return await req.Content.ReadAsStreamAsync();
        }

        public class AttachmentNotFoundException : ChannelSendFailedException
        {
        }

    }

    public class EmailRecvEvent
    {
        public MailgunEmailRaw email;
    }
}
