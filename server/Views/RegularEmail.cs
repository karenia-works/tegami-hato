using System;
using Karenia.TegamiHato.Server.Models;

namespace Karenia.TegamiHato.Server.Views
{
    public static class RegularEmailTemplate
    {
        public static readonly string template = @"
<!doctype html>
<head>
    <style>
        .mdi {
            font-size: 24px;
            margin: 16px;
        }

        .attachment-container {
            display: flex;
            flex-direction: column;
            align-items: flex-start;
        }

        .attachment {
            display: flex;
            flex-direction: row;
            align-items: center;
            width: auto;
            margin-bottom: 12px;
            padding: 8px;
            padding-left: 0px;
            border-radius: 4px;
            border: solid 2px #dbc6e0;
            min-width: 240px;
            max-width: 480px;
            color: inherit;
        }

        .attachment-filename {
            font-weight: bold;
        }

        .attachment-size {
            color: #8d7992;
        }

        .attachment-info {
            display: flex;
            flex-direction: column;
            align-items: flex-start;
        }

        .attachment-overflow {
            font-family: ""Sarasa UI SC"", ""Source Han Sans CN"", ""Noto Sans"",
                ""San Fransisco"", ""Segoe UI"", ""Roboto"", ""Noto Sans CJK SC"",
                ""Noto Sans SC"", ""Source Han Sans SC"", ""Microsoft Yahei UI"", sans-serif;
        }

        .attachment-overflow h2 {
            font-weight: 600;
            font-family: Poppins, ""Sarasa UI SC"", ""Source Han Sans CN"", ""Noto Sans"",
                ""San Fransisco"", ""Segoe UI"", ""Roboto"", ""Noto Sans CJK SC"",
                ""Noto Sans SC"", ""Source Han Sans SC"", ""Microsoft Yahei UI"", sans-serif;
            color: #59086d;
            margin-bottom: 12px;
        }
    </style>
    <link rel=""stylesheet"" href=""//cdn.materialdesignicons.com/4.9.95/css/materialdesignicons.min.css"">
</head>
<body>

<div class=""body"">
{{body}}
</div>

{{#if oversizedAttachments}}
<div class=""attachment-overflow"">
    <hr>
    <h2> Overflowed attachments </h2>
    <div class=""attachment-container"">
{{#each oversizedAttachments}}
        <a class=""attachment"" href=""{{this.url}}"">
            <div><span class=""mdi {{this.icon}}"" /></div>
            <div class=""attachment-info"">
                <div class=""attachment-filename"">
                    {{this.name}}
                </div>
                <div class=""attachment-size"">
                    {{this.size}}
                </div>
            </div>
        </a>
{{/each}}
    </div>
</div>
{{/if}}


<sub>
This email is sent from<a href= ""http://hato.karenia.cc"" > Tegami Hato</a>, a simple information broadcasting tool.
</sub>
</body>";

        public static readonly string templatePlaintext = @"
{{body}}

{{#if oversizedAttachments}}
---
## Oversized attachments ##

{{each oversizedAttachments}}
{{this.name}} ({{this.size}})
{{this.url}}

{{/each}}
{{/if}}

---
This email is sent from Tegami Hato, A Simple information broadcasting tool.
Learn more at http://hato.karenia.cc";


        // =========

        static RegularEmailTemplate()
        {
            compiledTemplate = HandlebarsDotNet.Handlebars.Compile(template);
            var htmlEncoder = HandlebarsDotNet.Handlebars.Configuration.TextEncoder;
            HandlebarsDotNet.Handlebars.Configuration.TextEncoder = new PlainTextEncoder();
            compiledTemplatePlaintext = HandlebarsDotNet.Handlebars.Compile(templatePlaintext);
            HandlebarsDotNet.Handlebars.Configuration.TextEncoder = htmlEncoder;
        }

        public static readonly Func<object, string> compiledTemplate;
        public static readonly Func<object, string> compiledTemplatePlaintext;
    }
}
