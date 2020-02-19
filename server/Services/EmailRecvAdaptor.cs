using System.Collections.Generic;
using System.Text.RegularExpressions;
using Karenia.TegamiHato.Server.Models;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.Logging;
using FluentEmail.Core.Models;
using System.IO;
using System.Net.Http;
using System.Collections;

namespace Karenia.TegamiHato.Server.Services
{
    public class EmailRecvAdaptor
    {
        public EmailRecvAdaptor(
            EmailRecvService recv,
            EmailSendingService send,
            ObjectStorageService oss,
            DatabaseService db,
            ILogger<EmailRecvAdaptor> logger)
        {
            this.recv = recv;
            this.send = send;
            this.oss = oss;
            this.db = db;
            this.logger = logger;
            this.AdaptServices();
        }

        private readonly EmailRecvService recv;
        private readonly EmailSendingService send;
        private readonly ObjectStorageService oss;
        private readonly DatabaseService db;
        private readonly ILogger logger;

        private HttpClient client = new HttpClient();

        public const int maxBodyLength = 300;

        private void AdaptServices()
        {
            this.recv.OnEmailRecv += this.OnEmailRecv;
        }

        private async void OnEmailRecv(EmailRecvEvent ev)
        {
            var email = ev.email;
            var match = Regex.Match(email.To, "(.*) <(?<name>.+)@.+>");
            var targetChannels = new List<string?>();
            var targetChannelIds = targetChannels.Select<string?, Ulid?>(str => { if (Ulid.TryParse(str, out Ulid res)) return res; else return null; }).Where(id => id != null).ToList();

            while (match.Success)
            {
                targetChannels.Add(match.Groups["name"].Value);
                match = match.NextMatch();
            }

            var origin = Regex.Match(email.From, "^(?<nick>.*) <(?<email>.+)>$").Groups;
            var senderEmail = origin["email"].Value;
            var senderNickname = origin["nick"].Value;

            var allTargetChannels = await this.db.db.Channels.Where(ch => targetChannelIds.Contains(ch.ChannelId)
            || targetChannels.Contains(ch.ChannelUsername)).ToListAsync();
            var allTargetChannelIds = allTargetChannels.Select(ch => ch.ChannelId).ToList();

            var msg = new HatoMessage()
            {
                MsgId = Ulid.Empty,
                // WARN: All timestamps represent the time when this email ARRIVES at the server
                // Timestamp = email.Date,
                SenderEmail = senderEmail,
                SenderNickname = senderNickname,
                Title = email.Subject,
                BodyHtml = email.BodyHtml,
                BodyPlain = email.BodyPlain,
            };

            var attachments = await UploadAndSaveAttachments(email);
            msg.Attachments = attachments;

            foreach (var channel in allTargetChannels)
            {
                msg._Channel = channel;
                msg.ChannelId = channel.ChannelId;
                await SaveEmail(msg);
            }

            List<IGrouping<string, string>> allTargetEmails = await GetEmailsFromChannelIds(allTargetChannelIds);

            await SendEmailAndFailure(senderEmail, senderNickname, msg, allTargetEmails);
            await db.db.SaveChangesAsync();
        }

        public async Task<List<HatoAttachment>> UploadAndSaveAttachments(MailgunEmailRaw email)
        {
            var atts = new List<HatoAttachment>();
            foreach (var att in email.Attachments)
            {
                var id = Ulid.NewUlid();
                var fileStream = await recv.GetAttachment(att.Url);
                var url = await oss.PutAttachment(id, att.Name, fileStream, att.Size, att.ContentType);
                var attachment = new HatoAttachment()
                {
                    AttachmentId = id,
                    Filename = att.Name,
                    Url = url,
                    ContentType = att.ContentType,
                    Size = att.Size,
                    IsAvailable = true
                };
                atts.Add(attachment);
            }
            await this.db.AddAttachmentEntries(atts);
            return atts;
        }


        public async Task SendEmailAndFailure(
            string senderEmail,
            string senderNickname,
            HatoMessage msg,
            List<IGrouping<string, string>> allTargetEmails)
        {
            var failedSendChannels = await SendEmail(msg, allTargetEmails);
            if (failedSendChannels.Count > 0)
            {
                string strippedBody;
                if (msg.BodyPlain.Length > maxBodyLength)
                {
                    strippedBody = msg.BodyPlain.Substring(0, 200) + "...";
                }
                else
                {
                    strippedBody = msg.BodyPlain;
                }

                foreach ((var name, var reason) in failedSendChannels)
                {
                    await SendFailureEmailBack(
                        senderEmail,
                        senderNickname,
                        name,
                        strippedBody,
                        reason,
                        msg.Timestamp);
                }

            }
        }

        public class CustomLittleGroup<TK, TV> : IGrouping<TK, TV>
        {
            public CustomLittleGroup(TK key, ICollection<TV> val)
            {
                this.key = key; this.val = val;
            }
            TK key;
            ICollection<TV> val;

            public TK Key => key;

            public IEnumerator<TV> GetEnumerator()
            {
                return val.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return val.GetEnumerator();
            }
        }

        public async Task<List<IGrouping<string, string>>> GetEmailsFromChannelIds(IList<Ulid> channelIds)
        {
            var allTargetEmails = await this.
               db.db.ChannelUserTable
               .Where(u => channelIds.Contains(u.ChannelId) && u.CanReceiveMessage)
               .Include(u => u._User)
               .Include(u => u._Channel)
               .ToListAsync();
            var resultEmails = allTargetEmails.GroupBy(
                entry => entry._Channel.ChannelUsername ?? entry.ChannelId.ToString(),
                entry => entry._User.Email
            ).ToList();
            return resultEmails;
        }

        public EmailData HatoMessageToEmailDataPartial(HatoMessage msg)
        {
            var data = new EmailData();

            data.FromAddress = new Address(
                $"{msg._Channel?.ChannelUsername ?? msg.ChannelId.ToString()}@{recv.Domain}",
                msg._Channel?.ChannelTitle);
            data.Subject = msg.Title;

            if (msg.BodyHtml != null)
            {
                data.IsHtml = true;
                data.Body = msg.BodyHtml;
                data.PlaintextAlternativeBody = msg.BodyPlain;
            }
            else
            {
                data.IsHtml = false;
                data.Body = msg.BodyPlain;
                data.PlaintextAlternativeBody = null;
            }
            return data;
        }

        private async Task SaveEmail(HatoMessage msg)
        {
            await this.db.SaveMessageIntoChannel(msg);
            foreach (var att in msg.Attachments)
            {
                this.db.db.AttachmentRelations.Add(new AttachmentMessageRelation()
                {
                    AttachmentId = att.AttachmentId,
                    MsgId = msg.MsgId,
                    RelId = Ulid.NewUlid()
                });
            }
        }



        public async Task<Stream> GetAttachment(string url)
        {
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                url = "http://" + url;
            var req = await client.GetAsync(url);
            return await req.Content.ReadAsStreamAsync();
        }


        /// <summary>
        /// Send email to target channels
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="targetEmails"></param>
        /// <returns>Email channels that failed to send</returns>
        public async Task<List<(string, string)>> SendEmail(HatoMessage msg, List<IGrouping<string, string>> targetEmails)
        {
            var failedChannels = new List<(string, string)>();
            foreach (var group in targetEmails)
            {
                var email = HatoMessageToEmailDataPartial(msg);
                if (group.Count() == 0) continue;

                email.Attachments = (await Task.WhenAll(msg.Attachments.Select(async att =>
                     new Attachment()
                     {
                         ContentId = att.AttachmentId.ToString(),
                         ContentType = att.ContentType,
                         Data = await GetAttachment(att.Url),
                         Filename = att.Filename,
                         IsInline = false
                     }))).ToList();

                var result = await send.SendEmail(email, group.ToList(), group.Key);
                if (!result.Successful)
                    failedChannels.Add(
                        (group.Key,
                        result.ErrorMessages.Aggregate(
                            new StringBuilder(),
                            (sb, msg) => sb.AppendLine(msg))
                        .ToString()));
            }
            return failedChannels;
        }

        public async Task SendFailureEmailBack(
            string target,
            string nick,
            string channelName,
            string summary,
            string reason,
            DateTimeOffset sendTime)
        {
            var timeRepr = sendTime.ToUniversalTime().ToString();
            var email = new FluentEmail.Core.Email($"{channelName}@{send.Domain}")
                .To(target, nick)
                // .ReplyTo(target)
                .Subject($"Failed to deliver your message to {channelName}")
                .Body($"<html><p>Your message sending to channel {channelName} at {timeRepr} was not delivered successfully.</p><p>Reason: {reason}.</p><p>Body of the failed message:</p><blockquote><pre>{summary}</pre></blockquote></html>", true)
                .PlaintextAlternativeBody($"Your message sending to channel {channelName} at {timeRepr} was not delivered successfully.\r\n\r\nReason: {reason}.\r\n\r\nBody of the failing message:\r\n{summary}")
                ;
            var resp = await this.send.SendEmail(email);
            if (!resp.Successful)
            {
                var sb = new StringBuilder();
                sb.AppendFormat("Failed to deliver failure message of channel {0} to {1}.", channelName, target);
                sb.AppendLine();
                sb.AppendLine("Messages:");
                foreach (var i in resp.ErrorMessages)
                {
                    sb.AppendFormat("  - {0}", i);
                }
                logger.LogWarning(sb.ToString());
            }
            else
            {
                logger.LogInformation($"Channel '{channelName}' not found message to {target} successfully sent");
            }
        }
    }
}
