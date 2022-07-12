using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SiteRippers
{
    public class Flexweb
    {
        private readonly string _user;
        private readonly string _password;

        public Flexweb(string user, string password)
        {
            _user = user;
            _password = password;
        }

        public async Task RipTo(string folder, DateTime start, DateTime end)
        {
            var httpMessageHandler = new HttpClientHandler
            {
                UseCookies = true,
            };
            var client = new HttpClient(httpMessageHandler);
            var response = await client.PostAsync("https://flexweb.kss.nl/login/login", new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", _user),
                new KeyValuePair<string, string>("password", _password),
                new KeyValuePair<string, string>("role", "7"),
            }));
            var loginMessage = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
            if (loginMessage.status == "error")
            {
                throw new Exception("Inloggen mislukt met de volgende melding: " + loginMessage.data);
            }

            var batch = new DateTime(start.Year, start.Month, 1);
            Console.Write("Bezig met downloaden van foto's");
            while (batch < end)
            {
                Console.Write(".");
                response = await client.PostAsync("https://flexweb.kss.nl/ouder/fotoalbum/standaardalbum", new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("year", batch.Year.ToString()),
                    new KeyValuePair<string, string>("month", batch.Month.ToString()),
                }));
                var fotos = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

                foreach (string foto in fotos.FOTOS)
                {
                    response = await client.PostAsync("https://flexweb.kss.nl/ouder/fotoalbum/fotometa", new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("id", foto),
                    }));
                    var meta = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
                    var dateTime = DateTime.Parse(meta[0].MEDIA_DAG.ToString());
                    if (dateTime < start || dateTime > end)
                        continue;
                    var mimeType = ((string)meta[0].MEDIA_MIMETYPE.ToString()).Split("/").Last();

                    var byteArray = await client.GetByteArrayAsync($"https://flexweb.kss.nl/ouder/media/download/media/{foto}");
                    File.WriteAllBytes(Path.Combine(folder, $"{dateTime:yyyyMMdd}_{foto}.{mimeType}"), byteArray);
                }

                batch = batch.AddMonths(1);
            }
            Console.WriteLine();
            Console.WriteLine("Klaar met downloaden");
        }
    }
}
