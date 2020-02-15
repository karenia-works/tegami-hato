using System.Text.Json.Serialization;
using System.Text.Json;
using NUlid;
using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;

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
        public override Ulid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            return Ulid.Parse(str);
        }

        public override void Write(Utf8JsonWriter writer, Ulid value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    // public class UlidTypeConverter : typecon
    // {

    // }
}
