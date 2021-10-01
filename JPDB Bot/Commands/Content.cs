using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace JPDB_Bot.Commands
{
    public class Content : BaseCommandModule
    {
        string statisticsPage = string.Empty;
        int uniqueWords = -1;

        [Command("content")]
        [Cooldown(3, 10, CooldownBucketType.User)]
        [Description("Search for content in the JPDB database and get statistics.\nFor example: !content steins gate")]
        public async Task SearchContent(CommandContext ctx,
            [DescriptionAttribute("Name of the content you are searching")] [RemainingText]
            string searchString)
        {
            Program.PrintCommandUse(ctx.User.Username, ctx.Message.Content);
            statisticsPage = string.Empty;
            uniqueWords = -1;
            //await ctx.Channel.SendMessageAsync("Searching for " + searchString + "...").ConfigureAwait(false);
            await ContentDetails(ctx, searchString);
            if (statisticsPage == string.Empty || uniqueWords == -1)
            {
                Program.PrintError("The program stopped before collecting content stats info.");
                return;
            }
            await ContentStats(ctx, searchString);
        }

        private async Task ContentDetails(CommandContext ctx,
            string searchString)
        {
            WebClient client = new WebClient();
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


            if (contentType != string.Empty) { contentType = "&show_only=" + contentType; }

            string url = "https://jpdb.io/prebuilt_decks?q=" + searchString + contentType;
            string html = "";
            try
            {
                html = client.DownloadString(new Uri(url));
            }
            catch (Exception ex)
            {
                Program.PrintError(ex.Message + $"\n(url)");
                return;
            }

            int snipIndex = html.IndexOf("30rem;\">") + 8;

            if (snipIndex == 7)
            {
                await ctx.RespondAsync("No content found UwU").ConfigureAwait(false);
                return;
            }

            string wordTemp = html.Substring(snipIndex);
            string wordTemp2 = html;
            html = wordTemp;

            snipIndex = wordTemp.IndexOf("<");
            html = wordTemp.Substring(snipIndex);
            string contentName = wordTemp.Substring(0, snipIndex);

            snipIndex = html.IndexOf("margin-top: 0.5rem;\">") + 22;
            wordTemp = html.Substring(snipIndex);
            snipIndex = wordTemp.IndexOf("/") + 1;
            wordTemp = wordTemp.Substring(snipIndex);
            snipIndex = wordTemp.IndexOf("\"");
            wordTemp = "https://jpdb.io/" + wordTemp.Substring(0, snipIndex);

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


            await ctx.RespondAsync("Found " + contentName + ":\n" + wordTemp).ConfigureAwait(false);
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
                html = client.DownloadString(new Uri(url));
            }
            catch (Exception ex)
            {
                Program.PrintError(ex.Message);
                return;
            }

            int snipIndex = html.IndexOf("data:") + 7;

            if (snipIndex == 7)
            {
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
                //80, 85, 90, 95, 97, 98
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
                else if (coverages.Count == 4 && Coverage > 97)
                {
                    coverages.Add(uniqueKnown);
                }
                else if (coverages.Count == 5 && Coverage > 98)
                {
                    coverages.Add(uniqueKnown);
                }

                if (uniqueKnown > 0 && uniqueKnown < 100) {
                    giniValue += (Coverage - (uniqueKnown / 100f)) / 99f;
                }

                uniqueKnown += 1;
            }

            //giniValue *= 100;
            Console.WriteLine($"Coverage rating: {giniValue}");

            giniValue = (giniValue - 60) * 3.0f;
            if (giniValue > 100) { giniValue = 100; }
            if (giniValue < 0) { giniValue = 0; }

            //80, 85, 90, 95, 97, 98
            string statsMessage = $"Coverage : Unique Words (/{uniqueWords})" +
                $"\n80% : {coverages[0]}% ({Math.Round(uniqueWords * ((float)coverages[0] / 100))} words)" +
                $"\n85% : {coverages[1]}% ({Math.Round(uniqueWords * ((float)coverages[1] / 100))} words)" +
                $"\n90% : {coverages[2]}% ({Math.Round(uniqueWords * ((float)coverages[2] / 100))} words)" +
                $"\n95% : {coverages[3]}% ({Math.Round(uniqueWords * ((float)coverages[3] / 100))} words)" +
                $"\n97% : {coverages[4]}% ({Math.Round(uniqueWords * ((float)coverages[4] / 100))} words)" +
                $"\n98% : {coverages[5]}% ({Math.Round(uniqueWords * ((float)coverages[5] / 100))} words)" +
                $"\nGini Coefficient: {Math.Round(giniValue * 100.0) / 100.0}%";
            //float test = wordCoverage[coverages[0]];


            await ctx.Channel.SendMessageAsync(statsMessage).ConfigureAwait(false);
        }
    }
}