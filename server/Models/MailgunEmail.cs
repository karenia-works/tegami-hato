using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;

namespace Karenia.TegamiHato.Server.Models
{
    public class MailgunEventList<T>
    {
        public List<T> Items { get; set; }

        //! Other fields omitted
    }

    public class MailgunEvent
    {
        public string Event { get; set; }
        public double Timestamp { get; set; }

        //! Other fields omitted
    }

    public class MailgunStorageEvent : MailgunEvent
    {
        public MailgunStorage Storage { get; set; }

        //! Other fields omitted
    }

    public class MailgunStorage
    {
        public string Url { get; set; }
        public string Key { get; set; }
    }

    public class MailgunEmailRecv
    {
        public string Recipients { get; set; }
        public string Sender { get; set; }
        public string From { get; set; }
        public string BodyHtml { get; set; }
        public string BodyPlain { get; set; }
        public string StrippedText { get; set; }
        public string StrippedSignature { get; set; }
        public List<MailgunAttachment> Attachments { get; set; }
        public string MessageHeaders { get; set; }
        public Dictionary<string, string> ContentIdMap { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement> extras { get; set; }
    }

    public class MailgunEmailRaw
    {
        public string To { get; set; }
        public string Recipients { get; set; }
        public DateTime Date { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public string BodyHtml { get; set; }
        public string BodyPlain { get; set; }
        public string StrippedText { get; set; }
        public string StrippedSignature { get; set; }
        public List<MailgunAttachment> Attachments { get; set; }
        // public List<string[]> MessageHeaders { get; set; }
        // public Dictionary<string, string> ContentIdMap { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement> extras { get; set; }
    }

    public class MailgunAttachment
    {
        public long Size { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
    }

    public class DateTimeSerializer : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return new Rfc822.DateTime(reader.GetString(), Rfc822.DateTimeSyntax.FourDigitYear | Rfc822.DateTimeSyntax.NumericTimeZone | Rfc822.DateTimeSyntax.WithDayName | Rfc822.DateTimeSyntax.WithSeconds).Instant.DateTime;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("r"));
        }
    }
}
