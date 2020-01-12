using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;
using FluentEmail.Core.Models;
using FluentEmail.Core;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Karenia.TegamiHato.Server.Services
{
    public class MailingChannelService
    {
        public MailingChannelService(EmailSendingService sendService, EmailRecvService recvService, ILogger<MailingChannelService> logger)
        {
            this.sendService = sendService;
            this.recvService = recvService;
            this.logger = logger;
            recvService.OnEmailRecv += this.dealWithChannel;
            recvService.beginEmailLoop();
        }

        private readonly ILogger<MailingChannelService> logger;

        private EmailSendingService sendService;
        private EmailRecvService recvService;

        private string testChannelName = "hatotest";
        private List<string> testChannelMembers = new List<string> {
            "lynzrand@outlook.com",
            "henryx@buaa.edu.cn",
        };

        public async void dealWithChannel(EmailRecvEvent ev)
        {
            var email = ev.email;
            var data = new EmailData();
            if (email.BodyHtml != null)
            {
                data.Body = email.BodyHtml;
                data.IsHtml = true;
            }
            else
            {
                data.Body = email.BodyPlain;
                data.IsHtml = false;
            }
            data.PlaintextAlternativeBody = email.BodyPlain;
            data.Subject = email.Subject;

            var captures = Regex.Match(email.To, "^(.*) <(?<name>.+)@.+>$").Groups;
            var channelName = captures["name"].Value;
            logger.LogInformation("Got email from {0} to {1}; channelName = {2}", email.From, email.To, channelName);

            var origin = Regex.Match(email.From, "^(?<nick>.*) <(?<email>.+)>$").Groups;
            var originEmail = origin["email"].Value;
            var originNick = origin["nick"].Value;
            try
            {
                foreach (var attachment in email.Attachments)
                {
                    var attachmentStream = await recvService.GetAttachment(attachment.Url);
                    var att = new Attachment();
                    att.Data = attachmentStream;
                    att.Filename = attachment.Name;
                    att.ContentType = attachment.ContentType;
                    data.Attachments.Add(att);
                }
                await this.SendEmailToChannel(channelName, data);
            }
            catch (ChannelSendFailedException e)
            {
                // const int maxBodyLength = 200;
                // string strippedBody;
                // if (email.BodyPlain.Length > maxBodyLength)
                // {
                //     strippedBody = email.BodyPlain.Substring(0, 200) + "...";
                // }
                // else
                // {
                //     strippedBody = email.BodyPlain;
                // }
                // await this.SendFailureEmailBack(
                //  originEmail, originNick, channelName, strippedBody, e.ToString(), email.Date);
                // logger.LogWarning(e, "Failed to send to channel");
            }
        }

        public async Task SendEmailToChannel(string channelName, EmailData emailData)
        {
            if (channelName == testChannelName)
            {
                emailData.FromAddress = new Address($"{channelName}@{sendService.Domain}");
                emailData.ToAddresses.AddRange(testChannelMembers.Select(name => new Address(name)));
                await this.sendService.SendEmail(emailData);
            }
            else
            {
                throw new ChannelNotFoundException() { ChannelName = channelName };
            }
        }

        public async Task SendFailureEmailBack(
            string target, string nick, string channelName, string summary, string reason, DateTime sendTime)
        {
            var timeRepr = sendTime.ToUniversalTime().ToString();
            var email = new Email($"{channelName}@{sendService.Domain}")
                .To(target, nick)
                // .ReplyTo(target)
                .Subject($"Failed to deliver your message to {channelName}")
                .Body($"<html><p>Your message sending to channel {channelName} at {timeRepr} was not delivered successfully.</p><p>Reason: {reason}.</p><p>Body of the failed message:</p><blockquote><pre>{summary}</pre></blockquote></html>", true)
                .PlaintextAlternativeBody($"Your message sending to channel {channelName} at {timeRepr} was not delivered successfully.\r\n\r\nReason: {reason}.\r\n\r\nBody of the failing message:\r\n{summary}")
                ;
            var resp = await this.sendService.SendEmail(email);
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
    public class ChannelSendFailedException : Exception
    {
        public override string ToString()
        {
            return $"Failed to send email to channel";
        }
    }

    public class ChannelNotFoundException : ChannelSendFailedException
    {
        public string ChannelName { get; set; }

        public override string ToString()
        {
            return $"Cannot find channel {ChannelName}";
        }
    }
}
