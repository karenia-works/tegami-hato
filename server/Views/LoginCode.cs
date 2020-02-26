using System;

namespace Karenia.TegamiHato.Server.Views
{
    public static class LoginCodeTemplate
    {
        public static readonly string template = @"
<!doctype html>
<head>
    <style>
        body {
           font-family: 'Source Han Sans CN', 'Sarasa UI SC', 'Noto Sans', 'San Fransisco', 'Segoe UI', 'Roboto', 'Noto Sans CJK SC', 'Noto Sans SC', 'Source Han Sans SC', 'Microsoft Yahei UI', sans-serif;
        }
        .login-code{
            font-family: 'Iosevka', 'IBM Plex Mono', 'Consolas', 'SF Mono', 'Roboto Mono', monospace;
            font-size: 2em;
            padding: 0.5em;
            text-align: center;
            background-color: #88888844;
            border-radius: 0.2em;
            max-width: 12em;
        }
        sub{
            font-size: 0.7em;
        }
    </style>
</head>
<body>
<h1>
    Login
</h1>
<p>
    Hi {{nickname}},
</p>
<p>
    We've seen you trying to log in just now. Here's your login code:
</p>
<pre class=""login-code"">
    {{code}}
</pre>
<p>
    If you haven't requested a login, please ignore this email. The code itself will expire in 15 minutes.
</p>

<sub>
This email is sent from<a href= ""http://hato.karenia.cc"" > Tegami Hato</a>, a simple information broadcasting tool.
</sub>
</body>";

        public static readonly string templatePlaintext = @"
##   Login   ##

Hi {{nickname}}, we've seen you trying to log in in just now. Here's your login code:

    >>>> {{code}} <<<<

If you haven't requested a login, please ignore or delete this email. The code expires in 15 minutes.

---
This email is sent from Tegami Hato, A Simple information broadcasting tool.
Learn more at http://hato.karenia.cc";


        // =========
        public static readonly Func<object, string> compiledTemplate = HandlebarsDotNet.Handlebars.Compile(template);
        public static readonly Func<object, string> compiledTemplatePlaintext = HandlebarsDotNet.Handlebars.Compile(templatePlaintext);
    }
}
