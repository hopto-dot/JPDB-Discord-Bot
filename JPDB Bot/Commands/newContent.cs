using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace JPDB_Bot.Commands
{
    public class newContent : BaseCommandModule
    {
        [Command("newcontent")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        [Description("See how much content is getting added in the next update")]
        public async Task newContentCommand(CommandContext ctx)
        {
            var handler = new HttpClientHandler();
            var client = new HttpClient(handler);
            var request = new HttpRequestMessage(new HttpMethod("GET"), "https://docs.google.com/spreadsheets/d/1XpwFYg9_EPVX8mMeTxWsqWCN_RQ_B7xuwyKrLKKnexE/edit");

            var response = await client.SendAsync(request);
            var html = response.Content.ReadAsStringAsync().Result;

            int snipIndex = -1;
            bool fail;

            //novels
            int novelsAdd = -1;
            snipIndex = html.IndexOf("<td class=\"s9\">") + 15;
            html = html.Substring(snipIndex);
            snipIndex = html.IndexOf(" ");
            fail = int.TryParse(html.Substring(0, snipIndex), out novelsAdd);
            if (fail == false) { await ctx.RespondAsync("Something went wrong with fetching the database information.").ConfigureAwait(false); Program.printError($"!newcontent failure: tried to parse '{html.Substring(0, snipIndex)}' (novelsAdd)"); return; }

            int novelsPreparing = -1;
            snipIndex = html.IndexOf("<td class=\"s10\">") + 16;
            html = html.Substring(snipIndex);
            snipIndex = html.IndexOf(" ");
            fail = int.TryParse(html.Substring(0, snipIndex), out novelsPreparing);
            if (fail == false) { await ctx.RespondAsync("Something went wrong with fetching the database information.").ConfigureAwait(false); Program.printError($"!newcontent failure: tried to parse '{html.Substring(0, snipIndex)}' (novelsPreparing)"); return; }


            /// dramas

            int dramasAdd = -1;
            snipIndex = html.IndexOf("<td class=\"s9\">") + 15;
            html = html.Substring(snipIndex);
            snipIndex = html.IndexOf(" ");
            fail = int.TryParse(html.Substring(0, snipIndex), out dramasAdd);
            if (fail == false) { await ctx.RespondAsync("Something went wrong with fetching the database information.").ConfigureAwait(false); Program.printError($"!newcontent failure: tried to parse '{html.Substring(0, snipIndex)}' (dramasAdd)"); return; }

            int dramasPreparing = -1;
            snipIndex = html.IndexOf("<td class=\"s10\">") + 16;
            html = html.Substring(snipIndex);
            snipIndex = html.IndexOf(" ");
            fail = int.TryParse(html.Substring(0, snipIndex), out dramasPreparing);
            if (fail == false) { await ctx.RespondAsync("Something went wrong with fetching the database information.").ConfigureAwait(false); Program.printError($"!newcontent failure: tried to parse '{html.Substring(0, snipIndex)}' (dramasPreparing)"); return; }

            var infoEmbed = new DiscordEmbedBuilder()
            {
                Title = "New Content",
                Description =
                $"__Novels__\n" +
                $"**Prepared:** {novelsAdd}\n" +
                $"**Preparing:** {novelsPreparing}\n" +
                $"\n" +
                $"__Dramas__\n" +
                $"**Prepared:** {dramasAdd}\n" +
                $"**Preparing:** {dramasPreparing}",
                Color = DiscordColor.Blue,
                Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = "How many of each type of content will be in the next update"
                },
                Url = "https://jpdb.io/prebuilt_decks",
            };

            //infoEmbed.AddField("prepared novels", novelsAdd.ToString(), false);
            //infoEmbed.AddField("prepared dramas", dramasAdd.ToString(), true);
            //infoEmbed.AddField("preparing novels", novelsPreparing.ToString(), false);
            //infoEmbed.AddField("preparing dramas", dramasPreparing.ToString(), true);

            try
            {
                var contentEmbedMessage = await ctx.Channel.SendMessageAsync(embed: infoEmbed).ConfigureAwait(false);
            }
            catch
            {
                Program.printError("Failed to send content embed message.");
                return;
            }


            Console.WriteLine();
        }


    }
}
