using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using NpgsqlTypes;
using System.Text.RegularExpressions;
using NUlid;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Marques.EFCore.SnakeCase;
using System.Text.Json.Serialization;
using FluentEmail.Core.Models;
using System.Linq;
using Karenia.TegamiHato.Server.Services;
using System.Threading.Tasks;
using System.Text;

namespace Karenia.TegamiHato.Server.Models
{
    // Disable initialization warning because we don't need that for now
#pragma warning disable CS8618

    public class HatoAttachment
    {
        [JsonConverter(typeof(UlidJsonConverter))]
        public Ulid AttachmentId { get; set; }
        public string Filename { get; set; }
        public string Url { get; set; }
        public string ContentType { get; set; }
        public long Size { get; set; }

        [JsonIgnore]
        public virtual HatoMessage? _HatoMessage { get; set; } = null;
    }

    public class HatoMessageAbbr
    {
        [JsonConverter(typeof(UlidJsonConverter))]
        public Ulid MsgId { get; set; }

        public HatoChannel? _Channel { get; set; } = null;

        [JsonConverter(typeof(UlidJsonConverter))]
        public Ulid ChannelId { get; set; }

        public DateTimeOffset Timestamp { get => ChannelId.Time; }

        public string SenderEmail { get; set; }
        public string? SenderNickname { get; set; }

        // public List<Address> Receivers { get; set; }

        public string Title { get; set; }

        public string BodyPlain { get; set; }
    }

    public class HatoMessage : HatoMessageAbbr
    {

        public string? BodyHtml { get; set; }

        public virtual ICollection<HatoAttachment> attachments { get; set; }

        [JsonIgnore]
        public NpgsqlTsVector? tsvector { get; set; }

        public async Task<EmailData> ToEmailData(EmailRecvService recvService)
        {
            var data = new EmailData();

            data.FromAddress = new Address(
                $"{_Channel?.ChannelUsername ?? this.ChannelId.ToString()}@{recvService.Domain}",
                _Channel?.ChannelTitle);

            if (BodyHtml != null)
            {
                data.IsHtml = true;
                data.Body = BodyHtml;
                data.PlaintextAlternativeBody = BodyPlain;
            }
            else
            {
                data.IsHtml = false;
                data.Body = BodyPlain;
                data.PlaintextAlternativeBody = null;
            }
            data.Attachments = (await Task.WhenAll(
                attachments
                .Select(async att => new Attachment()
                {
                    Filename = att.Filename,
                    ContentId = att.AttachmentId.ToString(),
                    ContentType = att.ContentType,
                    Data = await recvService.GetAttachment(att.Url)
                }))).ToList();
            return data;
        }
    }

    public class HatoChannel
    {
        [JsonConverter(typeof(UlidJsonConverter))]
        public Ulid ChannelId { get; set; }

        public string? ChannelUsername { get; set; }

        public string ChannelTitle { get; set; }

        public bool IsPublic { get; set; }

        public virtual ICollection<ChannelUserRelation> _Users { get; set; }
        public virtual ICollection<HatoMessage> _Messages { get; set; }
    }

    public class ChannelUserRelation
    {
        [JsonConverter(typeof(UlidJsonConverter))]
        public Ulid UserId { get; set; }
        public virtual User _User { get; set; }

        public Ulid ChannelId { get; set; }
        public virtual HatoChannel _Channel { get; set; }

        public bool CanSendMessage { get; set; }
        public bool CanReceiveMessage { get; set; }
        public bool CanEditRoles { get; set; }
    }

    public class User
    {
        [JsonConverter(typeof(UlidJsonConverter))]
        public Ulid UserId { get; set; }

        public string Nickname { get; set; }

        public string Email { get; set; }

        public virtual ICollection<ChannelUserRelation> _Channels { get; set; }

        [JsonIgnore]
        public virtual ICollection<UserLoginCode> _LoginCodes { get; set; } = null;
    }

    public class UserLoginCode
    {
        public Ulid CodeId { get; set; }

        public string Code { get; set; }

        public DateTimeOffset Expires { get; set; }

        // =============


        /// <summary>
        /// How long is the code?
        /// </summary>
        public const int CodeLength = 6;
        public static readonly char[] usableLetters = "0123456789ABCDEFGHJKMNPQRSTVWXYZ".ToCharArray();
        public static readonly TimeSpan ExpirationTime = new TimeSpan(0, 15, 0);

        public static UserLoginCode Generate(DateTimeOffset timestamp)
        {
            var rng = new System.Random();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < CodeLength; i++)
            {
                sb.Append(usableLetters[rng.Next(usableLetters.Length)]);
            }
            return new UserLoginCode()
            {
                CodeId = Ulid.NewUlid(),
                Code = sb.ToString(),
                Expires = timestamp + ExpirationTime
            };
        }
    }

    // Disable initialization warning because we don't need that for now

    public class EmailSystemContext : DbContext
    {
        public DbSet<HatoMessage> Messages { get; set; }
        public DbSet<HatoChannel> Channels { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ChannelUserRelation> ChannelUserTable { get; set; }
        public DbSet<HatoAttachment> Attachments { get; set; }
        // public DbSet<UserLoginCode> LoginCodes { get; set; }

        public EmailSystemContext(DbContextOptions ctx) : base(ctx)
        {
        }

        public static ValueConverter<Ulid, Guid> UlidGuidConverter = new ValueConverter<Ulid, Guid>(
            ulid => ulid.ToGuid(),
            guid => new Ulid(guid)
        );

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<HatoMessage>().HasKey(x => x.MsgId);
            modelBuilder.Entity<HatoMessage>().Property(x => x.MsgId).HasConversion(UlidGuidConverter);
            modelBuilder.Entity<HatoMessage>().HasOne(x => x._Channel).WithMany(x => x._Messages).HasForeignKey(x => x.ChannelId);
            modelBuilder.Entity<HatoMessage>().Property(x => x.ChannelId).HasConversion(UlidGuidConverter);
            modelBuilder.Entity<HatoMessage>().HasIndex(x => x.MsgId);
            modelBuilder.Entity<HatoMessage>().HasIndex(x => x.tsvector).HasMethod("GIN");

            modelBuilder.Entity<User>().HasKey(x => x.UserId);
            modelBuilder.Entity<User>().HasIndex(x => x.UserId);
            modelBuilder.Entity<User>().Property(x => x.UserId).HasConversion(UlidGuidConverter);
            modelBuilder.Entity<User>().HasAlternateKey(x => x.Email);
            modelBuilder.Entity<User>().HasIndex(x => x.Email);
            modelBuilder.Entity<User>().OwnsMany(x => x._LoginCodes, code =>
            {
                code.WithOwner().HasForeignKey("UserId");
                code.Property(x => x.CodeId).HasConversion(UlidGuidConverter);
                code.HasKey(x => x.CodeId);
                code.HasIndex(x => x.Code);
                code.HasIndex(x => x.Expires);
            });

            modelBuilder.Entity<HatoChannel>().HasKey(x => x.ChannelId);
            modelBuilder.Entity<HatoChannel>().HasIndex(x => x.ChannelId);
            modelBuilder.Entity<HatoChannel>().HasIndex(x => x.ChannelUsername);
            modelBuilder.Entity<HatoChannel>().Property(x => x.ChannelId).HasConversion(UlidGuidConverter);

            modelBuilder.Entity<HatoAttachment>().HasKey(x => x.AttachmentId);
            modelBuilder.Entity<HatoAttachment>().HasIndex(x => x.AttachmentId);
            modelBuilder.Entity<HatoAttachment>().Property(x => x.AttachmentId).HasConversion(UlidGuidConverter);
            modelBuilder.Entity<HatoAttachment>().HasOne(x => x._HatoMessage).WithMany(x => x.attachments);

            modelBuilder.Entity<ChannelUserRelation>().HasKey(x => new { x.UserId, x.ChannelId });
            modelBuilder.Entity<ChannelUserRelation>().HasIndex(x => new { x.UserId, x.ChannelId });
            modelBuilder.Entity<ChannelUserRelation>().HasIndex(x => x.UserId);
            modelBuilder.Entity<ChannelUserRelation>().HasIndex(x => x.ChannelId);
            modelBuilder.Entity<ChannelUserRelation>().HasOne(x => x._User).WithMany(u => u._Channels).HasForeignKey(u => u.UserId);
            modelBuilder.Entity<ChannelUserRelation>().HasOne(x => x._Channel).WithMany(u => u._Users).HasForeignKey(u => u.ChannelId);
            modelBuilder.Entity<ChannelUserRelation>().Property(x => x.UserId).HasConversion(UlidGuidConverter);
            modelBuilder.Entity<ChannelUserRelation>().Property(x => x.ChannelId).HasConversion(UlidGuidConverter);

            modelBuilder.ToSnakeCase();
        }
    }


    public static class StringExtensions
    {
        public static string ToSnakeCase(this string input)
        {
            if (string.IsNullOrEmpty(input)) { return input; }

            var startUnderscores = Regex.Match(input, @"^_+");
            return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
        }
    }
#pragma warning restore CS8618
}
