using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace DiscordBot.Commands
{
    public class TestCommands : BaseCommandModule
    {
        [Command("hi")]
        [Cooldown(2, 10, CooldownBucketType.User)]
        [Description("Get a nice response")]
        public async Task hi(CommandContext ctx)
        {
            Console.WriteLine(ctx.User.Username + " <- Hello");
            if (ctx.Member.Roles.Any(r => r.Name == "Owner" || r.Name == "Supporter" || r.Name == "Server Booster"))
            {
                await ctx.RespondAsync("Hello " + ctx.User.Username + "様 :)").ConfigureAwait(false);
            }
            else
            {
                await ctx.RespondAsync("Hello " + ctx.User.Username + "").ConfigureAwait(false);
            }
            
        }

        [Command("changelog")]
        [Cooldown(1, 20, CooldownBucketType.User)]
        [Description("Shows the latest addition to the changelog")]
        public async Task changeLog(CommandContext ctx)
        {
            string Date = String.Empty;

            /*WebClient Client = new WebClient();
            Client.Encoding = System.Text.Encoding.UTF8;
            string HTML = "";
            string sniptemp = string.Empty;
            HTML = Client.DownloadString(new Uri("https://jpdb.io/changelog"));
            int snipIndex = HTML.IndexOf("<h5 id=") + 8;
            HTML = HTML.Substring(snipIndex);
            snipIndex = HTML.IndexOf(">") + 1;
            HTML = HTML.Substring(snipIndex);

            sniptemp = HTML;
            snipIndex = sniptemp.IndexOf("<");
            sniptemp = sniptemp.Substring(0, snipIndex);

            var changelogEmbed = new discordembed*/

            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load("https://jpdb.io/changelog");
            string ChangeDate = htmlDoc.DocumentNode.SelectNodes("/html/body/div[2]/h5[1]")
    .First()
    .InnerHtml;
            string Information = htmlDoc.DocumentNode.SelectNodes("/html/body/div[2]/h5[1]").First().NextSibling.InnerHtml;

            await ctx.RespondAsync(ChangeDate).ConfigureAwait(false);
            Console.WriteLine(ctx.User.Username + " <- " + ChangeDate + " (Changelog)");
        }

        [Command("content")]
        [Cooldown(3, 10, CooldownBucketType.User)]
        [Description("Search for content in the JPDB database\nFor example: !content \"steins gate\"")]
        public async Task content(CommandContext ctx, [DescriptionAttribute("Name of the content you are searching")] string searchString)


        {
            Console.WriteLine(ctx.User.Username + " <- Searching for " + searchString + "...");
            //await ctx.Channel.SendMessageAsync("Searching for " + searchString + "...").ConfigureAwait(false);


            WebClient Client = new WebClient();
            Client.Encoding = System.Text.Encoding.UTF8;
            string HTML = "";
            int snipIndex = -1;

            string OriginalFilter = "anime";
            bool ScrapeKanji = false;
            

            if (HTML.Length > 250) return;

            int snipIndex2 = -1;

            bool pageDone = false;
            int pageInterval = 0;
            string wordTemp = "";
            string URL = "";
            List<string> wordIDs = new List<string>() { };
            // "?sort_by=by-frequency-local&offset="

                URL = "https://jpdb.io/prebuilt_decks?q=" + searchString;
                try
                {
                    HTML = Client.DownloadString(new Uri(URL));
                }
                catch (Exception ex)
                {
                    pageDone = true;
                }

                snipIndex = HTML.IndexOf("30rem;\">") + 8;
            if (snipIndex == 7)
            {
                await ctx.RespondAsync("No content found UwU").ConfigureAwait(false);
                return;
            }
                wordTemp = HTML.Substring(snipIndex);
                HTML = wordTemp;

                snipIndex = wordTemp.IndexOf("<");
                HTML = wordTemp.Substring(snipIndex);
                string contentName = wordTemp.Substring(0, snipIndex);

                snipIndex = HTML.IndexOf("margin-top: 0.5rem;\">") + 22;
                wordTemp = HTML.Substring(snipIndex);
                snipIndex = wordTemp.IndexOf("/") + 1;
                wordTemp = wordTemp.Substring(snipIndex);
                snipIndex = wordTemp.IndexOf("\"");
                wordTemp = "https://jpdb.io/" + wordTemp.Substring(0, snipIndex);

            await ctx.RespondAsync("Found " + contentName + ":\n" + wordTemp).ConfigureAwait(false);

            int Frequency = 1;
                    //if (Form1.PreserveFreq == true)
                    //{
                    //    // snipping to start of frequency
                    //    snipIndex = HTML.IndexOf("opacity: 0.5; margin-top: 1rem") + 34;
                    //    if (snipIndex == 33)
                    //        goto SkipFreq;
                    //    HTML = Strings.Mid(HTML, snipIndex);

                    //    // getting frequency
                    //    snipIndex = HTML.IndexOf("<");
                    //    Frequency = Strings.Left(HTML, snipIndex);

                    //    HTML = Strings.Mid(HTML, snipIndex);
                    //}
                    //SkipFreq:


                    if (wordTemp.Contains(">") == false && wordTemp.Contains("<") == false & wordTemp.Contains("=") == false & wordTemp.Contains("-") == false)
                    {
                        try
                        {
                            for (var I = 1; I <= Frequency; I++)
                                wordIDs.Add(wordTemp);
                        }
                        catch (Exception ex)
                        {
                        }
                    }

                    snipIndex = HTML.IndexOf("#a") + 3;


            // If Form1.cbbSearchType.Text = "Kanji" Then
            // Form1.ExtractKanji(wordIDs)
            // Return
            // End If

            if (wordIDs.Count > 1)
            {
                if (ScrapeKanji == false)
                {
                }
            }
        }
    }


}
