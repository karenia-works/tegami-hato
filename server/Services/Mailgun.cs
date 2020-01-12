using System.Net.Http;
using System.Collections;

namespace Karenia.TegamiHato.Server.Services
{
    public class MailgunService
    {
        static string mailgunBaseApi = "https://api.mailgun.net/v3/{0}";

        private string apiKey;
        private HttpClient client;

        public MailgunService(string apiKey)
        {
            this.apiKey = apiKey;
            this.client = new HttpClient();
        }

        public async void Test()
        {

        }
    }
}
