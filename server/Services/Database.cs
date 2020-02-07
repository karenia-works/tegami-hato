using System;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using Karenia.TegamiHato.Server.Models;
using System.Threading.Tasks;
using System.Linq;
using NUlid;
using System.Collections.Generic;

namespace Karenia.TegamiHato.Server.Services
{
    public class DatabaseService
    {
        public DatabaseService(EmailSystemContext db)
        {
            this.db = db;
        }

        public const int MaxResultPerQuery = 50;
        public EmailSystemContext db;


        public async Task<Guid> NewMailingChannel(string? channelName, string channelTitle)
        {
            var channelId = Ulid.NewUlid();
            var channel = new HatoChannel()
            {
                ChannelId = channelId,
                ChannelUsername = channelName,
                ChannelTitle = channelTitle,
            };
            var result = await db.Channels.AddAsync(channel);
            return channelId.ToGuid();
        }

        public async Task<IList<HatoMessage>> GetMessageFromChannel(Ulid channelId, Ulid start, int count)
        {
            if (count > MaxResultPerQuery)
                throw new ArgumentOutOfRangeException("count", count, $"A query can only check for at most {MaxResultPerQuery} results.");

            return await db.Messages
                .Where(
                    message =>
                        (message.ChannelId == channelId)
                        && (message.MsgId.CompareTo(start) > 0))
                .Take(count)
                .ToListAsync();
        }

        public async Task SaveMessageIntoChannel(HatoMessage message)
        {
            await SaveMessageIntoChannel(message, Ulid.Empty);
        }
        public async Task SaveMessageIntoChannel(HatoMessage message, Ulid channelId)
        {
            if (message.MsgId == Ulid.Empty)
            {
                message.MsgId = Ulid.NewUlid();
            }
            if (channelId != Ulid.Empty)
            {
                message.ChannelId = channelId;
            }

            await db.Messages.AddAsync(message);
        }
    }
}
