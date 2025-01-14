using DiscordBot.Modules.Monitor;
using DiscordBot.Modules.Pinterest;
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
            var result = await PinterestInteractionModule.GetContentFromPin(url);
            Console.WriteLine(result);
        }

        [TestCase(arguments: ["https://www.pinterest.ru/bastiennightheaven/dnd-decent-maps/"])]
        [TestCase(arguments: ["https://ru.pinterest.com/pin/790733647123200945/"])]
        [TestCase(arguments: ["https://ru.pinterest.com/pin/582301426866300054/"])]
        [TestCase(arguments: ["���, ��� ������ �� ��� https://pin.it/6FbMfBaEh, ������� ���� ����������"])]
        [TestCase(arguments: ["���, ��� ������ �� ��� pin.it/6FbMfBaEh, ������� ���� ����������"])]
        [TestCase(arguments: ["� ������ ��������� ����� �� �������. https://regex101.com/ https://ru.pinterest.com/pin/939141328529559079/ https://ru.pinterest.com/pin/967077719983180101/"])]
        public async Task ModifyMessageWithPinUrl(string msg)
        {
            var content = msg;
            string pattern = @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,4}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)";
            var matches = Regex.Matches(content, pattern);
            foreach (Match m in matches.Cast<Match>())
            {
                var getReplacement = await PinterestInteractionModule.GetContentFromPin(m.Value);
                content = Regex.Replace(content, m.Value, getReplacement);
            }

            Console.WriteLine($"Original: {msg}{Environment.NewLine}New: {content}");
        }

        [TestCase(arguments: ["� ������ ��������� ����� �� �������. https://regex101.com/ https://ru.pinterest.com/pin/939141328529559079/ https://ru.pinterest.com/pin/967077719983180101/"])]
        public void ExtractPinterestLinksTest(string text)
        {
            var result = PinterestModule.ExtractPinterestLinks(text);
            Console.WriteLine(string.Join(Environment.NewLine, result));
        }
    }
}