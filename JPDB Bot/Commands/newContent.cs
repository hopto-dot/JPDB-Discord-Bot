using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using JPDB_Bot;
using HtmlAgilityPack;
using DSharpPlus.SlashCommands;

namespace JPDB_Bot.Commands
{
    public class newContent : BaseCommandModule
    {
        int uses = 0;
        [Command("newcontent")]
        [Aliases("nc")]
        //[Cooldown(1, 10, CooldownBucketType.User)]
        [Description("See how much content will be added in the next content update")]
        public async Task newContentCommand(CommandContext ctx)
        {
            var handler = new HttpClientHandler();
            var client = new HttpClient(handler);
            var request = new HttpRequestMessage(new HttpMethod("GET"), Bot.configJson.ContributerSheetLink);

            var response = await client.SendAsync(request);
            var html = response.Content.ReadAsStringAsync().Result;

            int novelsAS = -1;
            int DramasAS = -1;
            int AnimeAS = -1;
            int VNsAS = -1;

            int novelsBP = -1;
            int DramasBP = -1;
            int AnimeBP = -1;
            int VNsBP = -1;
            try
            {
                html = snipStart(html, "<td class=\"s16\">");
                novelsAS = int.Parse(snipTo(html, " "));

                html = snipStart(html, "<td class=\"s16\">");
                DramasAS = int.Parse(snipTo(html, " "));

                html = snipStart(html, "<td class=\"s16\">");
                AnimeAS = int.Parse(snipTo(html, " "));

                html = snipStart(html, "<td class=\"s16\">");
                VNsAS = int.Parse(snipTo(html, " "));
                //////////////////////////////////////////////
                html = snipStart(html, "<td class=\"s17\">");
                novelsBP = int.Parse(snipTo(html, " "));

                html = snipStart(html, "<td class=\"s17\">");
                DramasBP = int.Parse(snipTo(html, " "));

                html = snipStart(html, "<td class=\"s17\">");
                AnimeBP = int.Parse(snipTo(html, " "));

                html = snipStart(html, "<td class=\"s17\">");
                VNsBP = int.Parse(snipTo(html, " "));
            }
             catch
            {
                await ctx.Message.RespondAsync("Something went wrong - Error code `JawGBoiShouldntBeUsingStringManipulation`");
                return;
            }

            var infoEmbed = new DiscordEmbedBuilder()
            {
                Title = "New Content",
                Description =
                $"__Novels__\n" +
                $"**Prepared:** {novelsAS}\n" +
                $"**Not ready yet:** {novelsBP}\n" +
                $"\n" +
                $"__Dramas__\n" +
                $"**Prepared:** {DramasAS}\n" +
                $"**Not ready yet:** {DramasBP}\n" +
                $"\n" +
                $"__Anime__\n" +
                $"**Prepared:** {AnimeAS}\n" +
                $"**Not ready yet:** {AnimeBP}\n" +
                $"\n" +
                $"__Visual Novels__\n" +
                $"**Prepared:** {VNsAS}\n" +
                $"**Not ready yet:** {VNsBP}\n",
                Color = DiscordColor.Green,
                Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = "How many of each type of content will be in the next content update"
                },
                Url = "https://jpdb.io/prebuilt_decks",
            };

            try
            {
                var contentEmbedMessage = await ctx.RespondAsync(embed: infoEmbed).ConfigureAwait(false);
            }
            catch
            {
                Program.printError("Failed to send newcontent embed message.");
                return;
            }


            Console.WriteLine();
        }

        private string snipStart(string HTML, string startString)
        {
            return HTML.Substring(HTML.IndexOf(startString) + startString.Length);
        }

        private string snipTo(string HTML, string endString)
        {
            return HTML.Substring(0, HTML.IndexOf(endString));
        }

    }

    public class newContentS : ApplicationCommandModule
    {
        [SlashCommand("newcontent", "shows live information for the next content update")]
        //[Cooldown(1, 10, CooldownBucketType.User)]
        public async Task newContentCommand(InteractionContext ctx)
        {
            var handler = new HttpClientHandler();
            var client = new HttpClient(handler);
            var request = new HttpRequestMessage(new HttpMethod("GET"), Bot.configJson.ContributerSheetLink);

            var response = await client.SendAsync(request);
            var html = response.Content.ReadAsStringAsync().Result;

            int novelsAS = -1;
            int DramasAS = -1;
            int AnimeAS = -1;
            int VNsAS = -1;

            int novelsBP = -1;
            int DramasBP = -1;
            int AnimeBP = -1;
            int VNsBP = -1;
            try
            {
                html = snipStart(html, "<td class=\"s16\">");
                novelsAS = int.Parse(snipTo(html, " "));

                html = snipStart(html, "<td class=\"s16\">");
                DramasAS = int.Parse(snipTo(html, " "));

                html = snipStart(html, "<td class=\"s16\">");
                AnimeAS = int.Parse(snipTo(html, " "));

                html = snipStart(html, "<td class=\"s16\">");
                VNsAS = int.Parse(snipTo(html, " "));
                //////////////////////////////////////////////
                html = snipStart(html, "<td class=\"s17\">");
                novelsBP = int.Parse(snipTo(html, " "));

                html = snipStart(html, "<td class=\"s17\">");
                DramasBP = int.Parse(snipTo(html, " "));

                html = snipStart(html, "<td class=\"s17\">");
                AnimeBP = int.Parse(snipTo(html, " "));

                html = snipStart(html, "<td class=\"s17\">");
                VNsBP = int.Parse(snipTo(html, " "));
            }
            catch
            {
                await ctx.CreateResponseAsync("Something went wrong - Error code `JawGBoiShouldntBeUsingStringManipulation`");
                return;
            }

            var infoEmbed = new DiscordEmbedBuilder()
            {
                Title = "New Content",
                Description =
                $"__Novels__\n" +
                $"**Prepared:** {novelsAS}\n" +
                $"**Not ready yet:** {novelsBP}\n" +
                $"\n" +
                $"__Dramas__\n" +
                $"**Prepared:** {DramasAS}\n" +
                $"**Not ready yet:** {DramasBP}\n" +
                $"\n" +
                $"__Anime__\n" +
                $"**Prepared:** {AnimeAS}\n" +
                $"**Not ready yet:** {AnimeBP}\n" +
                $"\n" +
                $"__Visual Novels__\n" +
                $"**Prepared:** {VNsAS}\n" +
                $"**Not ready yet:** {VNsBP}\n",
                Color = DiscordColor.Green,
                Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = "How many of each type of content will be in the next content update"
                },
                Url = "https://jpdb.io/prebuilt_decks",
            };

            try
            {
                await ctx.CreateResponseAsync(embed: infoEmbed).ConfigureAwait(false);
            }
            catch
            {
                Program.printError("Failed to send newcontent embed message.");
                return;
            }


            Console.WriteLine();
        }

        private string snipStart(string HTML, string startString)
        {
            return HTML.Substring(HTML.IndexOf(startString) + startString.Length);
        }

        private string snipTo(string HTML, string endString)
        {
            return HTML.Substring(0, HTML.IndexOf(endString));
        }

    }
}
