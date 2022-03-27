using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus;

namespace JPDB_Bot.Commands
{
    public class Content : BaseCommandModule
    {
        string statisticsPage = string.Empty;
        string contentName = string.Empty;
        string statsMessage = string.Empty;
        string imageURL = string.Empty;
        string contentURL = string.Empty;
        int page = -1;
        int uniqueWords = -1;
        public class TimedWebClient : WebClient
        {
            // Timeout in milliseconds, default = 600,000 msec
            public int Timeout { get; set; }

            public TimedWebClient()
            {
                this.Timeout = 600000;
            }

            protected override WebRequest GetWebRequest(Uri address)
            {
                var objWebRequest = base.GetWebRequest(address);
                objWebRequest.Timeout = this.Timeout;
                return objWebRequest;
            }
        }


        [Command("content")]
        [Cooldown(3, 10, CooldownBucketType.User)]
        [Description("Search for content in the JPDB database and get statistics.\nFor example: !content steins gate")]
        public async Task SearchContent(CommandContext ctx,
            [DescriptionAttribute("Name of the content you are searching")] [RemainingText]
            string searchString)
        {
            Program.printCommandUse(ctx.User.Username, ctx.Message.Content);
            statisticsPage = string.Empty;
            contentName = string.Empty;
            statsMessage = string.Empty;
            imageURL = string.Empty;
            contentURL = string.Empty;
            uniqueWords = -1;

            page = -1;

            for (int ep = 13; ep > 0;  ep -= 1)
            {
                if (searchString.Contains(" *" + ep) || searchString.Contains(" " + ep + "*") || searchString.Contains(" #" + ep))
                {
                    searchString = searchString.Replace(" *" + ep, "");
                    searchString = searchString.Replace(" #" + ep, "");
                    searchString = searchString.Replace(" " + ep + "*", "");

                    searchString = searchString.Trim();

                    page = ep * 10;
                }
            }
                

            //await ctx.Channel.SendMessageAsync("Searching for " + searchString + "...").ConfigureAwait(false);
            await ContentDetails(ctx, searchString).ConfigureAwait(false);
            //Program.printCommandUse(ctx.User.Username, "Content Stats");
            if (statisticsPage == string.Empty || uniqueWords == -1)
            {
                Program.printError("The program stopped before collecting content stats info.");
                return;
            }
            await ContentStats(ctx, searchString).ConfigureAwait(false);

            //Program.printCommandUse(ctx.User.Username, "Content Embed");
            await SendContentEmbed(ctx, true).ConfigureAwait(false);
        }


        private async Task SendContentEmbed(CommandContext ctx, Boolean test = false)
        {
            //Program.printError($"Content Embed:\n{contentName}\n{imageURL}\n{contentURL}");

            var myButton = new DiscordButtonComponent(ButtonStyle.Success, "my_custom_id", "Click for free vbucks");
            var builder = new DiscordMessageBuilder()
            .WithContent("This message isn't suspicious at all.")
            .AddComponents(myButton);
            //var testMessage = await ctx.Channel.SendMessageAsync(builder).ConfigureAwait(false);

            var embedThumbnail = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Url = imageURL,
            };

            string embedTitle = page == -1 ? contentName : contentName + $" #{page / 10}";

            var gameEmbed = new DiscordEmbedBuilder()
            {
                Title = embedTitle,
                Description = statsMessage,
                Color = DiscordColor.Red,
                Thumbnail = embedThumbnail,
                Url = contentURL,
                //Footer = new DiscordEmbedBuilder.EmbedFooter
                //{
                //    Text = "Currently, usernames don't do anything.",
                //}
            };
            
            try
            {
                var contentEmbedMessage = await ctx.Channel.SendMessageAsync(embed: gameEmbed).ConfigureAwait(false);
            } catch
            {
                Program.printError("Failed to send content embed message.");
                return;
            }
        }

        private async Task ContentDetails(CommandContext ctx,
            string searchString)
        {
            TimedWebClient client = new TimedWebClient();
            client.Encoding = System.Text.Encoding.UTF8;

            // Remove quotes if the search string is quoted ("stuff" -> stuff)
            if (searchString is { Length: >= 2 } && searchString[0] == '"' && searchString[^1] == '"')
            {
                searchString = searchString.Substring(1, searchString.Length - 2);
            }

            string contentType = string.Empty;
            if (searchString.Contains(" anime")) { contentType = "anime"; searchString = searchString.Replace(" anime", ""); }
            if (searchString.Contains(" ln")) { contentType = "novel"; searchString = searchString.Replace(" ln", ""); }
            if (searchString.Contains(" novel")) { contentType = "novel"; searchString = searchString.Replace(" novel", ""); }
            if (searchString.Contains(" vn")) { contentType = "visual_novel"; searchString = searchString.Replace(" vn", ""); }
            if (searchString.Contains(" game")) { contentType = "video_game"; searchString = searchString.Replace(" game", ""); }
            if (searchString.Contains(" wn")) { contentType = "web_novel"; searchString = searchString.Replace(" wn", ""); }

            if (contentType != string.Empty) { contentType = "&show_only=" + contentType; }

            string url = "https://jpdb.io/prebuilt_decks?q=" + searchString + contentType;
            string html = "";
            try
            {
                Program.printAPIUse("URL", url);
                html = new TimedWebClient { Timeout = 500 }.DownloadString(new Uri(url));
            }
            catch (Exception ex)
            {
                Program.printError(ex.Message + $"\n(url)");
                return;
            }

            string originalHTML = html;

            int snipIndex = html.IndexOf("30rem;\">") + 8;

            if (snipIndex == 7)
            {
                Program.printError("No content found.");
                await ctx.RespondAsync("No content found UwU").ConfigureAwait(false);
                return;
            }

            string wordTemp = html.Substring(snipIndex);
            string wordTemp2 = html;
            html = wordTemp;

            snipIndex = wordTemp.IndexOf("<");
            html = wordTemp.Substring(snipIndex);
            contentName = wordTemp.Substring(0, snipIndex).Replace("&#39;", "'");

            snipIndex = html.IndexOf("margin-top: 0.5rem;\">") + 22;
            wordTemp = html.Substring(snipIndex);
            snipIndex = wordTemp.IndexOf("/") + 1;
            wordTemp = wordTemp.Substring(snipIndex);
            snipIndex = wordTemp.IndexOf("\"");
            contentURL = "https://jpdb.io/" + wordTemp.Substring(0, snipIndex);

            string wordTemp3 = wordTemp2;
            //getting unique word count:
            snipIndex = wordTemp3.IndexOf("Unique words</th><td>") + 21;
            wordTemp3 = wordTemp3.Substring(snipIndex);
            snipIndex = wordTemp3.IndexOf("<");
            uniqueWords = int.Parse(wordTemp3.Substring(0, snipIndex));

            //getting statistics page:
            snipIndex = wordTemp2.IndexOf("dropdown-content") + 17;
            wordTemp2 = wordTemp2.Substring(snipIndex);
            snipIndex = wordTemp2.IndexOf("/");
            wordTemp2 = wordTemp2.Substring(snipIndex);
            snipIndex = wordTemp2.IndexOf("\"");
            wordTemp2 = wordTemp2.Substring(0, snipIndex);

            statisticsPage = "https://jpdb.io" + wordTemp2;

            wordTemp = originalHTML;
            snipIndex = wordTemp.IndexOf("lazy") + 2;
            wordTemp = wordTemp.Substring(snipIndex);
            snipIndex = wordTemp.IndexOf("rc=\"/static/") + 4;
            if (snipIndex != -1 && snipIndex < 9300)
            {
                wordTemp = wordTemp.Substring(snipIndex);
                snipIndex = wordTemp.IndexOf(".jpg") + 4;
                wordTemp = wordTemp.Substring(0, snipIndex);
                imageURL = "https://jpdb.io" + wordTemp;
                if (imageURL.Length > 42 || imageURL.Contains(".jpg") == false && imageURL.Contains("<") == false) { imageURL = ""; }
            }

            if (page != -1)
            {
                string contentURL = statisticsPage.Substring(0, statisticsPage.Length - 6);
                Program.printAPIUse("URL", contentURL);
                html = new TimedWebClient { Timeout = 500 }.DownloadString(new Uri(contentURL));

                snipIndex = html.IndexOf("display: flex; flex-wrap: wrap; margin-bottom: 1rem;");
                html = html.Substring(snipIndex + 10);

                bool done = false;
                int wa = 10;
                while (done == false)
                {
                    snipIndex = html.IndexOf("Unique words<");
                    if (snipIndex == -1) { done = true; continue; }
                    html = html.Substring(snipIndex + 21);

                    if (wa == page) { done = true; }

                    wa += 10;
                }

                try
                {
                    uniqueWords = int.Parse(html.Substring(0, html.IndexOf("<")));
                }
                catch { uniqueWords = -1; }
                

                Console.ResetColor();
            }



            
            //await ctx.RespondAsync("Found " + contentName + ":\n" + wordTemp).ConfigureAwait(false);
        }

        private async Task ContentStats(CommandContext ctx,
            string searchString)
        {
            WebClient client = new WebClient();
            client.Encoding = System.Text.Encoding.UTF8;

            // Remove quotes if the search string is quoted ("stuff" -> stuff)
            if (searchString is { Length: >= 2 } && searchString[0] == '"' && searchString[^1] == '"')
            {
                searchString = searchString.Substring(1, searchString.Length - 2);
            }

            string url = statisticsPage;
            string html = "";
            try
            {
                if (page != -1) { url = url.Replace("/stats", "/" + page + "/stats"); }
                html = client.DownloadString(new Uri(url));
            }
            catch (Exception ex)
            {
                Program.printError(ex.Message);
                return;
            }

            int snipIndex = html.IndexOf("data:") + 7;

            if (snipIndex == 7)
            {
                Program.printError("Failed to send content embed message.");
                await ctx.RespondAsync("Couldn't load statistics UwU").ConfigureAwait(false);
                return;
            }

            html = html.Substring(snipIndex);

            snipIndex = html.IndexOf("]");
            html = html.Substring(0, snipIndex);

            string[] wordCoverageString = html.Split(",");
            float[] wordCoverage = Array.ConvertAll<string, float>(wordCoverageString, float.Parse);

            List<int> coverages = new List<int>();
            int uniqueKnown = 0;

            float giniValue = 0;

            foreach (float Coverage in wordCoverage)
            {
                //80, 85, 90, 95, 96, 97, 98
                if (coverages.Count == 0 && Coverage > 80)
                {
                    coverages.Add(uniqueKnown);
                }
                else if (coverages.Count == 1 && Coverage > 85)
                {
                    coverages.Add(uniqueKnown);
                }
                else if (coverages.Count == 2 && Coverage > 90)
                {
                    coverages.Add(uniqueKnown);
                }
                else if (coverages.Count == 3 && Coverage > 95)
                {
                    coverages.Add(uniqueKnown);
                }
                else if (coverages.Count == 4 && Coverage > 96)
                {
                    coverages.Add(uniqueKnown);
                }
                else if (coverages.Count == 5 && Coverage > 97)
                {
                    coverages.Add(uniqueKnown);
                }
                else if (coverages.Count == 6 && Coverage > 98)
                {
                    coverages.Add(uniqueKnown);
                }

                if (uniqueKnown > 0 && uniqueKnown < 100) {
                    giniValue += (Coverage - (uniqueKnown / 100f)) / 99f;
                }

                uniqueKnown += 1;
            }

            giniValue = (giniValue - 50) * 2;

            float coverageRating = (giniValue - 50) * 3f;
            coverageRating = (float)(Math.Round(coverageRating * 100.0) / 100.0);
            decimal cmodifier = Math.Round((decimal)(wordCoverage[8] + 45) / 100, 2);
            if (cmodifier < 0.7m) { cmodifier = 0.7m; }
            
            coverageRating = (float)((decimal)coverageRating * cmodifier);
            if (coverageRating < 10) { coverageRating = 10; }

            string coverageStars = string.Empty;
            int stars = (int)Math.Floor(coverageRating / 10);
            if (stars > 5) { stars = 5; }
            
            for (int star = 0; stars != star; star += 1)
            {
                if (star%2 == 1)
                {
                    coverageStars += "★";
                }
                else if (star % 2 == 0 && star == stars - 1)
                {
                    coverageStars += "⯨";
                }
            }

            if (giniValue > 100) { giniValue = 100; }
            if (giniValue < 0) { giniValue = 0; }

            //80, 85, 90, 95, 96, 97, 98
            statsMessage = $"Coverage : Unique Words (/{uniqueWords})" +
                $"\n80% : {coverages[0]}% ({Math.Round(uniqueWords * ((float)coverages[0] / 100))} words)" +
                $"\n85% : {coverages[1]}% ({Math.Round(uniqueWords * ((float)coverages[1] / 100))} words)" +
                $"\n90% : {coverages[2]}% ({Math.Round(uniqueWords * ((float)coverages[2] / 100))} words)" +
                $"\n95% : {coverages[3]}% ({Math.Round(uniqueWords * ((float)coverages[3] / 100))} words)" +
                $"\n96% : {coverages[4]}% ({Math.Round(uniqueWords * ((float)coverages[4] / 100))} words)" +
                $"\n97% : {coverages[5]}% ({Math.Round(uniqueWords * ((float)coverages[5] / 100))} words)" +
                $"\n98% : {coverages[6]}% ({Math.Round(uniqueWords * ((float)coverages[6] / 100))} words)" +
                $"\nGini Value: {giniValue}%" +
                $"\nCoverage Rating: {stars + 1}/10★" +
                $"\nJump offset: {cmodifier}";
            //float test = wordCoverage[coverages[0]];


            //await ctx.Channel.SendMessageAsync(statsMessage).ConfigureAwait(false);
        }
    }
}