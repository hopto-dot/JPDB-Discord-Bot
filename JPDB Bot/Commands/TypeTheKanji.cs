using System;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using Newtonsoft.Json;
using JPDB_Bot;
using System.Drawing;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
//using static JPDB_Bot.Commands.TypeTheKanji;

namespace JPDB_Bot.Commands
{
    public class TypeTheKanji : BaseCommandModule
    {
        public class kanji
        {
            public string name = "";
            public int frequency = -1;
        }

        public class player
        {
            public DiscordMember member;
            public int points = 0;
        }

        
        [Command("typethekanji")]
        [Aliases("ttk")]
        [Cooldown(1, 5, CooldownBucketType.Channel)]
        [Description("Type the kanji game. Type the kanji shown in the image\n\n__Difficulties__:\n * **N5** - 0-100\n * **Easy** - 100-500\n * **Normal** - 500-1000\n * **Medium** - 1000-1500\n * **Hard** - 1500-2000\n * **Very Hard** - 2000-3000\n * **Kanji Deity** - 3000-4000\n\n * **Jouyou Kanji**")]
        public async Task typeTheKanji(CommandContext ctx, string difficulty = "hard")
        {
            if (difficulty == "skip" || difficulty == "stop") { return; }
            difficulty = difficulty.Trim();

            int bottomFreq = 0;
            int topFreq = -1;
            int answerTime = 15000;
            string kanjiFile = "kanjis.json";
            if (!difficulty.Contains("-"))
            {
                switch (difficulty.ToLower().Trim())
                {
                    case "n5":
                        topFreq = 100;
                        answerTime = 14000;
                        difficulty = $"N5 ({bottomFreq}-{topFreq})";
                        break;
                    case "n4":
                        bottomFreq = 100;
                        topFreq = 300;
                        answerTime = 14000;
                        difficulty = $"N4 ({bottomFreq}-{topFreq})";
                        break;
                    case "easy":
                        bottomFreq = 100;
                        topFreq = 500;
                        answerTime = 14000;
                        difficulty = $"Easy ({bottomFreq}-{topFreq})";
                        break;
                    case "n3":
                        bottomFreq = 200;
                        topFreq = 650;
                        answerTime = 15000;
                        difficulty = $"N3 ({bottomFreq}-{topFreq})";
                        break;
                    case "normal":
                    case "n2":
                        bottomFreq = 600;
                        topFreq = 1000;
                        answerTime = 16000;
                        difficulty = $"N2 ({bottomFreq}-{topFreq})";
                        break;
                    case "medium":
                        bottomFreq = 1000;
                        topFreq = 1500;
                        answerTime = 17000;
                        difficulty = $"Medium ({bottomFreq}-{topFreq})";
                        break;
                    case "hard":
                        bottomFreq = 1500;
                        topFreq = 2000;
                        answerTime = 18000;
                        difficulty = $"Hard ({bottomFreq}-{topFreq})";
                        break;
                    case "n1":
                        bottomFreq = 1500;
                        topFreq = 2000;
                        answerTime = 19000;
                        difficulty = $"N1 ({bottomFreq}-{topFreq})";
                        break;
                    case "very hard":
                    case "veryhard":
                    case "very":
                        bottomFreq = 2000;
                        topFreq = 3000;
                        answerTime = 20000;
                        difficulty = $"Very Hard ({bottomFreq}-{topFreq})";
                        break;
                    case "kanjideity":
                    case "kanji deity":
                    case "deity":
                    case "kanjigod":
                    case "kanji god":
                    case "kanji":
                    case "god":
                        bottomFreq = 3000;
                        topFreq = 4000;
                        answerTime = 23000;
                        difficulty = $"Kanji Deity ({bottomFreq}-{topFreq})";
                        break;
                    case "jouyou":
                    case "常用漢字":
                    case "常用":
                        bottomFreq = 1000;
                        topFreq = 4000;
                        answerTime = 17000;
                        difficulty = $"Jouyou Kanji";
                        kanjiFile = $"jouyoukanjis.json";
                        break;
                    default:
                        topFreq = 100;
                        difficulty = $"N5 ({bottomFreq}-{topFreq})";
                        break;
                }
            }
            else
            {
                if (difficulty.Substring(difficulty.Length - 1, 1) == "-" || difficulty.Substring(0, 1) == "-")
                {
                    await ctx.RespondAsync("You must provide a frequency range in the format `lower-upper`!").ConfigureAwait(false); return;
                }
                string[] difficultySplit = difficulty.Split("-");
                if (difficultySplit.Length != 2)
                {
                    await ctx.RespondAsync("You must provide a frequency range in the format `lower-upper`!").ConfigureAwait(false); return;
                }
                try
                {
                    bottomFreq = int.Parse(difficultySplit[0]);
                    topFreq = int.Parse(difficultySplit[1]);
                } catch
                {
                    await ctx.RespondAsync("You must provide integers for the frequency range!").ConfigureAwait(false); return;
                }
            }

            string jsonString = File.ReadAllText(kanjiFile);
            List<kanji> kanjis = new List<kanji>();
            try
            {
                kanjis = JsonConvert.DeserializeObject<List<kanji>>(jsonString);
            }
            catch
            {
                await ctx.RespondAsync("Couldn't start the game as no kanji information file was detected.").ConfigureAwait(false);
                return;
            }

            #region use font
            FontFamily fontFamily; //"みつフォント", "青柳疎石フォント2 OTF", "HG行書体", "UD Digi Kyokasho N-R"
            try
            {
                fontFamily = new FontFamily("UD Digi Kyokasho N-R");
            }
            catch
            {
                try
                {
                    fontFamily = new FontFamily("HG行書体");
                }
                catch
                {
                    try
                    {
                        fontFamily = new FontFamily("Arial");
                    }
                    catch
                    {
                        await ctx.RespondAsync("Something went wrong with getting the font");
                        return;
                    }
                }
            }
            #endregion

            #region send start game embed
            var infoEmbed = new DiscordEmbedBuilder()
            {
                Title = $"Type The Kanji - Difficulty {difficulty}",
                Description = "Starting a Type The Kanji game...",
                Color = DiscordColor.Blue,
                Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = "Type the kanji (not reading) shown in the image"
                },
                Url = "https://jpdb.io/kanji-by-frequency",
            };

            try
            {
                var contentEmbedMessage = await ctx.RespondAsync(embed: infoEmbed).ConfigureAwait(false);
            }
            catch
            {
                Program.printError("Failed to send start game embed.");
                return;
            }
            #endregion
            //System.Threading.Thread.Sleep(5000);
            if (waitFor(ctx, "", 5000) == "stop")
            {
                goto endGame;
            }

            await startGame(bottomFreq, topFreq, kanjis, fontFamily, ctx, answerTime);

endGame:
            await ctx.Channel.SendMessageAsync($"Game finished!").ConfigureAwait(false);
        }

        List<player> players = new List<player>();
        private async Task startGame(int bottomFreq, int topFreq, List<kanji> kanjis, FontFamily fontFamily, CommandContext ctx, int answerTime = 10000) 
        {
            Font newFont = new Font(fontFamily, 80, FontStyle.Regular, GraphicsUnit.Pixel);

            List<int> pickedFreqs = new List<int>();
            if (topFreq > kanjis.Count - 1) { topFreq = kanjis.Count - 1; }
            int points = 0;
            int consecutiveFails = 0;
            var random = new Random((int)DateTime.UtcNow.Ticks);

            for (int i = 1; i <= 10; i++)
            {
pickFreq:
                if (consecutiveFails == 3) { goto endGame; }
                int pickedFreq = -1;
                try
                {
                    pickedFreq = random.Next(bottomFreq, topFreq + 1);
                }
                catch { goto pickFreq; }

                foreach (int freq in pickedFreqs)
                {
                    if (pickedFreqs.Count >= topFreq - bottomFreq + 1)
                    { await ctx.Channel.SendMessageAsync("You have gone through all the kanji in the provided range!").ConfigureAwait(false); goto endGame; }
                    else if (freq == pickedFreq) { goto pickFreq; }
                }
                pickedFreqs.Add(pickedFreq);
                string kanji = kanjis[pickedFreq].name;

                #region Generate image and send
                Image generatedImage = drawText(kanji, newFont, Color.Black, Color.White);
                try { generatedImage.Save("KanjiImage.jpg"); }
                catch
                {
                    await ctx.RespondAsync("Something went wrong with generating the image");
                    return;
                }
                
                
                var fileStream = File.OpenRead("KanjiImage.jpg");

                DiscordMessageBuilder discordMessageBuilder = new DiscordMessageBuilder();
                discordMessageBuilder.WithFile(fileStream);
                await discordMessageBuilder.SendAsync(ctx.Channel);
                fileStream.Dispose();
                #endregion

                string response = waitFor(ctx, kanji, answerTime);
                //possible responses:
                //stop, skip, correct, "" (correct kanji not given)
                if (response == "stop")
                { return; }
                else if (response == "skip")
                {
                    if (pickedFreqs.Count >= topFreq - bottomFreq + 1)
                    {
                        await ctx.Channel.SendMessageAsync("You have gone through all the kanji!").ConfigureAwait(false); goto endGame;
                    }
                    else
                    {
                        kanjiInfoEmbed(ctx, false, kanji, kanjis[pickedFreq].frequency, "No one");
                        //await ctx.Channel.SendMessageAsync($"No one got it right, the answer was {kanji} which has a frequency of {kanjis[pickedFreq].frequency}.\nScore is {(points == 0 ? "" : "still")} {points}!").ConfigureAwait(false);
                        consecutiveFails++;
                        //System.Threading.Thread.Sleep(5000);
                        if (waitFor(ctx, "", 5000) == "stop") { return; }

                        continue;
                    }
                }
                else if (response != "") //if someone got the kanji right (response should be set to their username)
                {
                    points += 1;
                    kanjiInfoEmbed(ctx, true, kanji, kanjis[pickedFreq].frequency, response);
                    //await ctx.Channel.SendMessageAsync($"**{result.Result.Author.Username}** got it right, score is now {points}!\n{kanji} has a frequency of {kanjis[pickedFreq].frequency}").ConfigureAwait(false);
                    consecutiveFails = 0;
                }
                else //
                {
                    kanjiInfoEmbed(ctx, false, kanji, kanjis[pickedFreq].frequency, "No one");
                    //await ctx.Channel.SendMessageAsync($"No one got it right, the answer was {kanji} which has a frequency of {kanjis[pickedFreq].frequency}").ConfigureAwait(false);
                    //System.Threading.Thread.Sleep(1500);
                    if (waitFor(ctx, "", 1500) == "stop") { return; }
                    if (pickedFreqs.Count >= topFreq - bottomFreq + 1)
                    { await ctx.Channel.SendMessageAsync("You have gone through all the kanji!").ConfigureAwait(false); goto endGame; }

                    consecutiveFails++;
                }

                if (pickedFreqs.Count >= topFreq - bottomFreq + 1)
                { await ctx.Channel.SendMessageAsync("You have gone through all the kanji!").ConfigureAwait(false); goto endGame; }
                if (i != 10)
                {
                    //System.Threading.Thread.Sleep(4000);
                    if (waitFor(ctx, "", 4000) == "stop") { return; }
                }
            }

        endGame:
            var gameEmbed = new DiscordEmbedBuilder()
            {
                Title = "Game finished",
                Description = $"Score: {points}/10",
                Color = DiscordColor.Red
            };

            try
            {
                var contentEmbedMessage = await ctx.Channel.SendMessageAsync(embed: gameEmbed).ConfigureAwait(false);
            }
            catch
            {
                Program.printError("Failed to send content embed message.");
                return;
            }
        }

        private void kanjiInfoEmbed(CommandContext ctx, bool correct, string kanji, int frequency, string correctUser = "%username%", string meanings = "...")
        {
            string scoreString = "";
            foreach (player player in players)
            {
                scoreString += $"\n{player.member.Username}: {player.points}";
            }

            string embedTitle = correctUser == "%username%" || correctUser.ToLower() == "no one" ? embedTitle = $"No one got it right\nThe answer was {kanji}!" : $"{correctUser} got it right!\nThe answer was {kanji}";
            DiscordColor embedColour = correct ? DiscordColor.Green : DiscordColor.Red;
            var infoEmbed = new DiscordEmbedBuilder()
            {
                Title = embedTitle,
                Description = "Meanings: ...\n" +
                $"Frequency: {frequency}",
                Color = embedColour,
                Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = scoreString
                },
                Url = "https://jpdb.io/kanji-by-frequency",
            };

            try
            {
                var contentEmbedMessage = ctx.RespondAsync(embed: infoEmbed).ConfigureAwait(false);
            }
            catch
            {
                Program.printError("Failed to send start game embed.");
                ctx.Channel.SendMessageAsync("Something went wrong.").ConfigureAwait(false);
                return;
            }
        }

        [Command("grabkanji")]
        [Hidden]
        public async Task grabKanji(CommandContext ctx)
        {
            if (ctx.Message.Author.Id != 630381088404930560) { return; }
            WebClient client = new WebClient();
            string html = client.DownloadString("https://jpdb.io/kanji-by-frequency?show_only=jouyou");
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);

            var kanjis = document.DocumentNode.SelectNodes("/html[1]/body[1]/div[2]/table[1]")[0].ChildNodes;
            int i = 1;
            List<kanji> kanjiList = new List<kanji>();
            foreach (var kanji in kanjis)
            {
                //if (kanji[])
                if (kanji.ChildNodes[1].Name == "th") { continue; }
                if (kanji.ChildNodes[1].ChildNodes[0].Attributes == null) { continue; }

                string kanjiName = "";
                if (kanji.ChildNodes[1].ChildNodes[0].Attributes[0].Value == "font-size: 200%;")
                {
                    kanjiName = kanji.ChildNodes[1].ChildNodes[0].InnerText;
                }
                kanji newKanji = new kanji();
                newKanji.name = kanjiName;
                newKanji.frequency = int.Parse(kanji.ChildNodes[0].InnerText.Replace(".", ""));
                kanjiList.Add(newKanji);
                i++;


            }

            string jsonString = JsonConvert.SerializeObject(kanjiList);
            File.WriteAllText("jouyoukanjis.json", jsonString);
        }

        public string waitFor(CommandContext ctx, string kanji, int waitTime)
        {
            InteractivityResult<DiscordMessage> result = new InteractivityResult<DiscordMessage>();
            if (kanji == "") //if the function has been called for waiting (excluding kanji)
            {
                result = ctx.Channel.GetNextMessageAsync(
                    message =>
                       message.Content == "!stop"
                    || message.Content == "!ttk stop"
                    , TimeSpan.FromMilliseconds(waitTime)).Result;
            }
            else //if the function has been called for waiting (including kanji)
            {
                result = ctx.Channel.GetNextMessageAsync(
                    message =>
                       message.Content.Contains(kanji) == true
                    ||message.Content == "!stop"
                    || message.Content == "!ttk stop"

                    || message.Content == "!skip"
                    || message.Content == "!ttk skip"
                    , TimeSpan.FromMilliseconds(waitTime)).Result;
            }

            if (result.Result != null)
            {
                if (result.Result.Content == "!stop" || result.Result.Content == "!ttk stop")
                { return "stop"; }
                if (result.Result.Content == "!skip" || result.Result.Content == "!ttk skip")
                { return "skip"; }
                if (kanji != "" && result.Result.Content.Contains(kanji))
                {
                    return result.Result.Author.Username;
                }
            }
            
            
            return "";
        }

        public static Image drawText(String text, Font font, Color textColor, Color backColor)
        {
            //first, create a dummy bitmap just to get a graphics object
            Image img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);

            //measure the string to see how big the image needs to be
            SizeF textSize = drawing.MeasureString(text, font);

            //free up the dummy image and old graphics object
            img.Dispose();
            drawing.Dispose();

            //create a new image of the right size
            img = new Bitmap((int)textSize.Width, (int)textSize.Height);

            drawing = Graphics.FromImage(img);

            //paint the background
            drawing.Clear(backColor);

            //create a brush for the text
            Brush textBrush = new SolidBrush(textColor);

            drawing.DrawString(text, font, textBrush, 0, 0);

            drawing.Save();

            textBrush.Dispose();
            drawing.Dispose();

            return img;

        }
    }

    

}





