using System.Collections.Generic;
using System.Text.RegularExpressions;
using Karenia.TegamiHato.Server.Models;
using System.Linq;
using NUlid;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Karenia.TegamiHato.Server.Services
{
    public class EmailRecvAdaptor
    {
        public EmailRecvAdaptor(
            EmailRecvService recv,
            EmailSendingService send,
            DatabaseService db,
            ILogger<EmailRecvAdaptor> logger
        )
        {
            this.recv = recv;
            this.send = send;
            this.db = db;
            this.logger = logger;
            this.AdaptServices();
        }

        private readonly EmailRecvService recv;
        private readonly EmailSendingService send;
        private readonly DatabaseService db;
        private readonly ILogger logger;

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

            var allTargetChannels = await this.db.db.Channels.Where(ch => targetChannelIds.Contains(ch.ChannelId) || targetChannels.Contains(ch.ChannelUsername)).ToListAsync();
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

            foreach (var channel in allTargetChannels)
            {
                msg._Channel = channel;
                msg.ChannelId = channel.ChannelId;
                await SaveEmail(msg);
            }

            var allTargetEmails = await this.
                db.db.ChannelUserTable
                .Where(u => allTargetChannelIds.Contains(u.ChannelId))
                .Include(entry => entry._User)
                .GroupBy(
                    entry => entry._Channel.ChannelUsername ?? entry.ChannelId.ToString(),
                    entry => entry._User.Email)
                .ToListAsync();

            var failedSendChannels = await SendEmail(msg, allTargetEmails);
            if (failedSendChannels.Count > 0)
            {
                string strippedBody;
                if (email.BodyPlain.Length > maxBodyLength)
                {
                    strippedBody = email.BodyPlain.Substring(0, 200) + "...";
                }
                else
                {
                    strippedBody = email.BodyPlain;
                }

                foreach ((var name, var reason) in failedSendChannels)
                {
                    await SendFailureEmailBack(
                        senderEmail,
                        senderNickname,
                        name,
                        strippedBody,
                        reason,
                        email.Date);
                }

            }
        }


        private async Task SaveEmail(HatoMessage msg)
        {
            await this.db.SaveMessageIntoChannel(msg);
        }

        /// <summary>
        /// Send email to target channels
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="targetEmails"></param>
        /// <returns>Email channels that failed to send</returns>
        private async Task<List<(string, string)>> SendEmail(HatoMessage msg, List<IGrouping<string, string>> targetEmails)
        {
            var failedChannels = new List<(string, string)>();
            foreach (var group in targetEmails)
            {
                var email = await msg.ToEmailData(recv);
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
            DateTime sendTime)
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
