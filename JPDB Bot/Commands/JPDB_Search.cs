using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JPDB_Bot.Commands
{
    public class JPDB_Search : BaseCommandModule
    {
        public class Content
        {
            public string ContentName = string.Empty;
            public string deckURL = string.Empty;
            public string imageURL = string.Empty;
            public string[] Genres = { };
            public int difficulty = -1;
        }


        [Command("recommend")]
        [Cooldown(1, 10, CooldownBucketType.Channel)]
        [Description("Get a recommendation of content in the JPDB database")]
        public async Task recommend(CommandContext ctx, string difficulty = "-1")
        {
            loadDatabaseContent();
        }

        public Content[] loadDatabaseContent()
        {
            if (File.Exists("Content.txt") == false)
            {
                Program.printError("No database content to load");
                return null;
            }

            string contentFile = File.ReadAllText("Content.txt");
            JObject o1 = JObject.Parse(contentFile);

            return null;
        }



        [Command("scan")]
        [Cooldown(1, 20, CooldownBucketType.Channel)]
        [Description("Search for content in the JPDB database and get statistics.\nFor example: !content steins gate")]
        public async Task scanDatabase(CommandContext ctx,
            [DescriptionAttribute("Type of scan")] [RemainingText]
            string scanType = "novels")
        {
            if (ctx.User.Id.ToString() != "630381088404930560" || ctx.User.Id.ToString() == "118408957416046593" || ctx.User.Id.ToString() == "399993082806009856" || ctx.User.Id.ToString() == "245371520174522368" || ctx.User.Id.ToString() == "246834752328302593")
            {
                var gameEmbed = new DiscordEmbedBuilder()
                {
                    Title = "User not permitted",
                    Description = "❌ You are not permitted to start a scan of the database",
                    Color = DiscordColor.Red,
                };
                var contentEmbedMessage = await ctx.Channel.SendMessageAsync(embed: gameEmbed).ConfigureAwait(false);
                return;
            }

            startScan(ctx, scanType);
        }

        private void startScan(CommandContext ctx, string scanType = "novels")
        {
            string mediaType = "light_novel";
            scanType = scanType.Replace("s", "");
            if (scanType == "vn" || scanType == "visual novel" || scanType == "visualnovel" || scanType == "visual_novel") { mediaType = "visual_novel"; }
            else if (scanType == "anime" || scanType == "a") { mediaType = "anime"; }
            else if (scanType == "wn" || scanType == "web" || scanType == "web novel") { mediaType = "web_novel"; }
            else if (scanType == "la" || scanType == "live" || scanType == "live action") { mediaType = "live_action"; }
            else if (scanType == "all") { mediaType = "all"; }

            ctx.Channel.SendMessageAsync($"Scanning {mediaType}s".Replace("alls", "all content")).ConfigureAwait(false);


            bool finished = false;
            int pageScrape = 0;

            List<Content> fullList = new List<Content>();
            int lastCount = -1;
            while (finished == false) {
                lastCount = fullList.Count;
                fullList.AddRange(scrapePage(ctx, pageScrape, mediaType));
                if (lastCount == fullList.Count) { finished = true; continue; }
                pageScrape += 50;
                System.Threading.Thread.Sleep(900);
            }

            

            string outputString = "{";
            int i = 0;
            foreach (Content content in fullList)
            {
                if (i != 0) { outputString += ","; }
                outputString += "\n\t{\n";
                outputString += $"\t\"Name\": \"{content.ContentName}\",\n";
                outputString += $"\t\"Image URL\": \"{content.imageURL}\",\n";
                outputString += $"\t\"Difficulty\": \"{content.difficulty}\",\n";
                outputString += $"\t\"Deck URL\": \"{content.deckURL}\"\n";
                outputString += "\t}";
                i += 1;
            }
            outputString += "\n}";
            File.WriteAllText("Content.txt" , outputString);

            ctx.Channel.SendMessageAsync($"✅ Successfully scanned {fullList.Count} {mediaType}s".Replace("alls", "everything")).ConfigureAwait(false);
            Program.printMessage("Finished scan");
        }

        private List<Content> scrapePage(CommandContext ctx, int pageNo, string scanType = "novels")
        {
            string url = string.Empty;
            if (scanType != "all")
            {
                url= $"https://jpdb.io/prebuilt_decks?show_only={scanType}&sort_by=difficulty&offset={pageNo}#a";
            }
            else
            {
                url = $"https://jpdb.io/prebuilt_decks?sort_by=difficulty&offset={pageNo}#a";
            }
            

            //url = "https://google.co.uk";
            Program.printAPIUse($"Scrape {pageNo}", url);

            WebClient client = new WebClient();
            client.Encoding = System.Text.Encoding.UTF8;
            string html = string.Empty;
            string htmlTemp = html;

            try
            {
                html = client.DownloadString(new Uri(url));
            }
            catch (Exception ex)
            {
                Program.printError(ex.Message);
                return null;
            }

            int snipIndex = -1;

            List<Content> pageContents = new List<Content>();
            bool finished = false;
            while (html.Contains("max-width: 30rem;") == true && finished == false)
            {
                Content newContent = new Content();

                html = html.Substring(snipIndex + 5); //getting html to start at the right place

                //snipping imageURL
                snipIndex = html.IndexOf("loading=\"lazy\" src=\""); //loading="lazy" src=\"
                if (snipIndex != -1)
                {
                    html = html.Substring(snipIndex);
                    html = html.Substring(20);
                    htmlTemp = html; //html now starts with Name of content, about to snip just the name:
                    snipIndex = html.IndexOf("\"");
                    if (snipIndex == -1) { finished = true; continue; }
                    string imageURL = html.Substring(0, snipIndex);
                    if (imageURL.Length > 50) { finished = true; Program.printError($"imageURL = {imageURL}"); continue; }
                    newContent.imageURL = "https://jpdb.io" + imageURL;
                }
                else { finished = true; continue; }

                //snipping name
                snipIndex = html.IndexOf("max-width: 30rem;");
                if (snipIndex == -1) { finished = true; continue; }
                html = html.Substring(snipIndex);
                html = html.Substring(19);
                htmlTemp = html; //html now starts with Name of content, about to snip just the name:
                snipIndex = html.IndexOf("<");
                if (snipIndex == -1) { finished = true; continue; }
                string contentName = html.Substring(0, snipIndex);
                if (contentName.Length > 250) { finished = true; Program.printError($"contentName = {contentName}"); continue; }
                newContent.ContentName = contentName.Replace("&#39;", "'");

                //snipping difficulty
                snipIndex = html.IndexOf("Difficulty</th><td>");
                if (snipIndex == -1) { finished = true; continue; }
                html = html.Substring(snipIndex);
                html = html.Substring(19);
                htmlTemp = html; //html now starts with Name of content, about to snip just the name:
                snipIndex = html.IndexOf("/");
                if (snipIndex == -1) { finished = true; continue; }
                string difficulty = html.Substring(0, snipIndex);
                try { newContent.difficulty = int.Parse(difficulty); } catch { finished = true; Program.printError($"difficulty = {difficulty}"); continue; }

                //snipping deck URL
                snipIndex = html.IndexOf("margin-top: 0.5rem;\"><a href=\"");
                if (snipIndex == -1) { finished = true; continue; }
                html = html.Substring(snipIndex);
                html = html.Substring(30);
                htmlTemp = html; //html now starts with Name of content, about to snip just the name:
                snipIndex = html.IndexOf("\"");
                if (snipIndex == -1) { finished = true; continue; }
                string deckURL = html.Substring(0, snipIndex);
                if (deckURL.Length > 150) { finished = true; Program.printError($"deckURL.length = {deckURL.Length}"); continue; }
                newContent.deckURL = "https://jpdb.io" + deckURL;

                html = html.Substring(snipIndex + 5); //getting html to start at the right place
                Program.printMessage(contentName);

                pageContents.Add(newContent);
            }

           return pageContents;
        }

    }

}