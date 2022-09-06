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

namespace JPDB_Bot.Commands
{
    public class newContent : BaseCommandModule
    {
        int uses = 0;
        [Command("newcontent")]
        [Aliases("nc")]
        //[Cooldown(1, 10, CooldownBucketType.User)]
        [Description("See how much content is getting added in the next update")]
        public async Task newContentCommand(CommandContext ctx)
        {
            if (uses == 0)
            {
                await ctx.RespondAsync("To stop further cyberbullying I will put this command on hold until JawGBoi fixes it").ConfigureAwait(false);
            }
            else if (uses == 1)
            {
                await ctx.RespondAsync("**To stop further cyberbullying I will put this command on hold until JawGBoi fixes it**").ConfigureAwait(false);
            }
            else if (uses == 2)
            {
                await ctx.RespondAsync("Please stop :)").ConfigureAwait(false);
            }
            else if (uses == 3)
            {
                await ctx.RespondAsync("I said stop :)").ConfigureAwait(false);
            }
            else if (uses == 4)
            {
                await ctx.RespondAsync("Stop it.").ConfigureAwait(false);
            }
            else if (uses == 5)
            {
                await ctx.RespondAsync("Stop it..").ConfigureAwait(false);
            }
            else if (uses >= 6 && uses <= 8)
            {
                await ctx.RespondAsync("Stop it...").ConfigureAwait(false);
            }
            else if (uses == 9)
            {
                await ctx.RespondAsync("I'm not your friend anymore.").ConfigureAwait(false);
            }
            else if (uses == 10)
            {
                var msg = await ctx.Guild.GetMemberAsync(ctx.Message.Author.Id).Result.SendMessageAsync("I hate you -.-");
                Program.printMessage($"DM'ed @{ctx.Message.Author.Username} 'I hate you -.-' with msg id {msg.Id}");
            }
            else
            {
                
            }
            uses++;
            return;

            //////
            var handler = new HttpClientHandler();
            var client = new HttpClient(handler);
            var request = new HttpRequestMessage(new HttpMethod("GET"), Bot.configJson.ContributerSheetLink);

            var response = await client.SendAsync(request);
            var html = response.Content.ReadAsStringAsync().Result;

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);

            var scriptNodeChildren = document.DocumentNode.ChildNodes[1].ChildNodes[1].ChildNodes;
            HtmlNode cellsNode = null;
            foreach (var node in scriptNodeChildren)
            {
                if (node.Name == "div" & node.Attributes != null)
                {
                    Console.WriteLine();
                    if (node.Attributes.Count == 2)
                    {
                        if (node.Attributes[0].Value == "docs-editor-container")
                        {
                            cellsNode = node;
                            goto foundNode; //please don't scold me
                        }
                    }
                }
                Console.WriteLine();
            }
            if (cellsNode == null) { return; }
foundNode:
            cellsNode = cellsNode.ChildNodes[0].ChildNodes[5];
            Console.WriteLine();

            getCell(cellsNode, "S15");


            //var infoEmbed = new DiscordEmbedBuilder()
            //{
            //    Title = "New Content",
            //    Description =
            //    $"__Novels__\n" +
            //    $"**Prepared:** {novelsAdd}\n" +
            //    $"**Not ready yet:** {novelsPreparing}\n" +
            //    $"\n" +
            //    $"__Dramas__\n" +
            //    $"**Prepared:** {dramasAdd}\n" +
            //    $"**Not ready yet:** {dramasPreparing}\n" +
            //    $"\n" +
            //    $"__Anime__\n" +
            //    $"**Prepared:** {animeAdd}\n" +
            //    $"**Not ready yet:** {animePreparing}",
            //    Color = DiscordColor.Blue,
            //    Footer = new DiscordEmbedBuilder.EmbedFooter()
            //    {
            //        Text = "How many of each type of content will be in the next update"
            //    },
            //    Url = "https://jpdb.io/prebuilt_decks",
            //};

            //infoEmbed.AddField("prepared novels", novelsAdd.ToString(), false);
            //infoEmbed.AddField("prepared dramas", dramasAdd.ToString(), true);
            //infoEmbed.AddField("preparing novels", novelsPreparing.ToString(), false);
            //infoEmbed.AddField("preparing dramas", dramasPreparing.ToString(), true);

            try
            {
                //var contentEmbedMessage = await ctx.RespondAsync(embed: infoEmbed).ConfigureAwait(false);
            }
            catch
            {
                Program.printError("Failed to send content embed message.");
                return;
            }


            Console.WriteLine();
        }

        private int getCell(HtmlNode node, string className)
        {
            Console.WriteLine();
            node = node.ChildNodes[0].ChildNodes[4].ChildNodes[0].ChildNodes[0].ChildNodes[0].ChildNodes[1];

            foreach (var cell in node.ChildNodes)
            {
                if (cell.ChildNodes.Count < 2) { continue; }
                if (cell.ChildNodes[1] == null || cell.ChildNodes[1].Attributes.Count == 0) { continue; }
                if (cell.ChildNodes[1].Attributes["class"].Value.Contains("s16"))
                {
                    Console.WriteLine();
                }
            }
            //var test = node.ChildNodes.ToList().Where(c => c.ChildNodes[1].Attributes["class"].Value.Contains("s6"));


            return 0;
        }

    }
}
