using System.Text.Json.Serialization;
using System.Text.Json;
using System;
using HandlebarsDotNet;

namespace Karenia.TegamiHato.Server.Models
{
    public class ErrorResult
    {
        public ErrorResult(string error, string? reason = null, string? context = null)
        {
            this.Error = error;
            this.Reason = reason;
            this.Context = context;
        }
        public string Error { get; set; }
        public string? Reason { get; set; }
        public object? Context { get; set; }
    }

    public class UlidJsonConverter : JsonConverter<Ulid>
    {
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public override Ulid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            return Ulid.Parse(str);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public override void Write(Utf8JsonWriter writer, Ulid value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    public struct PlainTextEncoder : HandlebarsDotNet.ITextEncoder
    {
        public string Encode(string value)
        {
            return value;
        }
    }
}
