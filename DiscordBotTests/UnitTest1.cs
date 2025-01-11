using HtmlAgilityPack;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using System;
using System.Net.NetworkInformation;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using static System.Net.WebRequestMethods;

namespace DiscordBotTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }

        [TestCase(arguments: ["https://pin.it/2EEOkdF5H"])]
        [TestCase(arguments: ["https://pin.it/1VTyeCjTF"])]
        [TestCase(arguments: ["https://pin.it/2wxN2vdKB"])]
        [TestCase(arguments: ["https://www.google.ru/"])]
        [TestCase(arguments: ["asdd"])]
        [TestCase(arguments: ["https://ru.pinterest.com/pin/840343611753455428/"])]
        public async Task GetContentFromPinTest(string url)
        {
            var result = await GetContentFromPin(url);
            Console.WriteLine(result);
        }

        private async Task<string> GetContentFromPin(string url)
        {
            var result = string.Empty;

            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var document = new HtmlDocument();
                        document.LoadHtml(content);

                        var nodes = document.DocumentNode
                            .SelectNodes("//video|//img");
                        var node = nodes.First();
                        var imgSrc = node.Attributes["src"].Value;
                        result = node.Name switch
                        {
                            "img" => node.Attributes["src"].Value,
                            "video" => node.Attributes["src"].Value.Replace("hls", "720p").Replace("m3u8", "mp4"),
                            _ => throw new NotImplementedException($"Not implemented exception of {node.Name} element"),
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return result;
        }

        [TestCase(arguments: ["https://www.pinterest.ru/bastiennightheaven/dnd-decent-maps/"])]
        [TestCase(arguments: ["https://ru.pinterest.com/pin/790733647123200945/"])]
        [TestCase(arguments: ["https://ru.pinterest.com/pin/582301426866300054/"])]
        [TestCase(arguments: ["Ого, это ссылка на пин https://pin.it/6FbMfBaEh, которую надо распарсить"])]
        [TestCase(arguments: ["Ого, это ссылка на пин pin.it/6FbMfBaEh, которую надо распарсить"])]
        [TestCase(arguments: ["Я скинул несколько пинов на парсинг. https://regex101.com/ https://ru.pinterest.com/pin/939141328529559079/ https://ru.pinterest.com/pin/967077719983180101/"])]
        public async Task ModifyMessageWithPinUrl(string msg)
        {
            var content = msg;
            string pattern = @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,4}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)";
            var matches = Regex.Matches(content, pattern);
            foreach (Match m in matches)
            {
                var getReplacement = await GetContentFromPin(m.Value);
                content = Regex.Replace(content, m.Value, getReplacement);
            }

            Console.WriteLine($"Original: {msg}{Environment.NewLine}New: {content}");
        }

        [TestCase(arguments: ["Я скинул несколько пинов на парсинг. https://regex101.com/ https://ru.pinterest.com/pin/939141328529559079/ https://ru.pinterest.com/pin/967077719983180101/"])]
        public void ExtractPinterestLinksTest(string text)
        {
            var result = ExtractPinterestLinks(text);
            Console.WriteLine(string.Join(Environment.NewLine, result));
        }

        public IEnumerable<string> ExtractPinterestLinks(string text)
        {
            string[] whiteList = [
                "pinterest.com",
                "pinterest.ca",
                "pinterest.co.uk",
                "pinterest.fr",
                "pinterest.de",
                "pinterest.es",
                "pin.it",
                "pinterest.com.au",
                "pinterest.ph",
                "pinterest.ch",
                "pinterest.com.mx",
                "pinterest.dk",
                "pinterest.pt",
                "pinterest.ru",
                "pinterest.it",
                "pinterest.at",
                "pinterest.jp",
                "pinterest.cl",
                "pinterest.ie",
                "pinterest.co.kr",
                "pinterest.nz",
                "pintrest.com",
                "pinterest100.com",
                "pinterestcareers.com",
                "pinterst.com",
                "pinimg.com",
                "pinterest.vn",
                "pinterest.co",
                "pinterest.com.uy",
                "pinterest.com.pe",
                "pinterest.nl",
                "lecker-in-bestform.de",
                "pinterestmail.com",
                "pinterest.co.id",
                "pinterestpresents.co.uk",
                "pinterestpresents.de",
                "pinterest-mail.com",
                "pinterestpresents.com.au",
                "winterhus.ch",
                "pinterestpredicts.com",
                "pintrist.com",
                "traeumezimmer.ch",
                "pinterest.com.py",
                "http://pinterest.co.nz/pinterest.be",
                "pinterest.hu",
                "pintergration.com",
                "pinterest.kr",
                "pinterest.co.in",
                "pinterest.pe",
                "pinterest.engineering",
                "pinttest.com",
                "pintereststatus.com",
                "pinterest.uk",
                "pinterest.com.vn",
                "pinterest.tw",
                "pinterest.mx",
                "pinterest.com.ec",
                "pinterest.biz",
                "pinterest.com.bo",
                "pintertools.com",
                "querybook.org",
                "pinterest.ec",
                "pinterest.id",
                "pinterest.in",
                "pinterest.th",
                "pinterest.co.at",
                "pinterest.com.pt",
                "pinterest.info"];

            string pattern = @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,4}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)";
            var matches = Regex.Matches(text, pattern);
            var result = from m in matches
                         where whiteList.Any(x => m.Value.Contains(x))
                         select m.Value;
            return result;
        }
    }
}