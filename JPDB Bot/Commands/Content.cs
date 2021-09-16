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
        [Command("content")]
        [Cooldown(3, 10, CooldownBucketType.User)]
        [Description("Search for content in the JPDB database\nFor example: !content \"steins gate\"")]
        public async Task SearchContent(CommandContext ctx,
            [DescriptionAttribute("Name of the content you are searching")]
            string searchString)
        {
            Program.PrintCommandUse(ctx.User.Username, ctx.Message.Content);
            //await ctx.Channel.SendMessageAsync("Searching for " + searchString + "...").ConfigureAwait(false);

            WebClient client = new WebClient();
            client.Encoding = System.Text.Encoding.UTF8;

            string url = "https://jpdb.io/prebuilt_decks?q=" + searchString;
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

            int snipIndex = html.IndexOf("30rem;\">") + 8;

            if (snipIndex == 7)
            {
                await ctx.RespondAsync("No content found UwU").ConfigureAwait(false);
                return;
            }

            string wordTemp = html.Substring(snipIndex);
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

            await ctx.RespondAsync("Found " + contentName + ":\n" + wordTemp).ConfigureAwait(false);

            /*
            int Frequency = 1;
            List<string> wordIDs = new List<string>() { };
            if (wordTemp.Contains(">") == false && wordTemp.Contains("<") == false & wordTemp.Contains("=") == false &
                wordTemp.Contains("-") == false)
            {
                try
                {
                    for (var I = 1; I <= Frequency; I++)
                        wordIDs.Add(wordTemp);
                }
                catch (Exception ex)
                {
                    Program.PrintError(ex.Message);
                    return;
                }
            }
            */
        }
    }
}