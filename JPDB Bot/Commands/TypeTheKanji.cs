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
        
        [Command("typethekanji")]
        [Aliases("ttk")]
        [Description("Type the kanji game. Type the kanji shown in the image\n\n__Difficulties__:\n * **N5** - 0-100\n * **Easy** - 100-500\n * **Normal** - 500-1000\n * **Medium** - 1000-1500\n * **Hard** - 1500-2000\n * **Very Hard** - 2000-3000\n * **Kanji Deity** - 3000-4000\n\n * **Jouyou Kanji**")]
        public async Task typeTheKanji(CommandContext ctx, string difficulty)
        {
            difficulty = difficulty.Trim();

            int bottomFreq = 0;
            int topFreq = -1;
            int answerTime = 12000;
            string kanjiFile = "kanjis.json";
            if (!difficulty.Contains("-"))
            {
                switch (difficulty.ToLower().Trim())
                {
                    case "n5":
                        topFreq = 100;
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
                        answerTime = 14000;
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
                        answerTime = 20000;
                        difficulty = $"Hard ({bottomFreq}-{topFreq})";
                        break;
                    case "n1":
                        bottomFreq = 1500;
                        topFreq = 2000;
                        answerTime = 20000;
                        difficulty = $"N1 ({bottomFreq}-{topFreq})";
                        break;
                    case "very hard":
                    case "veryhard":
                    case "very":
                        bottomFreq = 2000;
                        topFreq = 3000;
                        answerTime = 25000;
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
                        answerTime = 25000;
                        difficulty = $"Kanji Deity ({bottomFreq}-{topFreq})";
                        break;
                    case "jouyou":
                    case "常用漢字":
                    case "常用":
                        bottomFreq = 0;
                        topFreq = 4000;
                        answerTime = 15000;
                        difficulty = $"Jouyou Kanji";
                        kanjiFile = $"jouyoukanjis.json";
                        break;
                    default:
                        topFreq = 100;
                        difficulty = $"N5 ({bottomFreq}-{topFreq})";
                        break;
                }
            } else
            {
                if (difficulty.Substring(difficulty.Length - 1, 1) == "-" || difficulty.Substring(0, 1) == "-")
                {
                    await ctx.RespondAsync("You must a frequency range in the format `lower-upper`!").ConfigureAwait(false); return;
                }
                string[] difficultySplit = difficulty.Split("-");
                if (difficultySplit.Length != 2)
                {
                    await ctx.RespondAsync("You must a frequency range in the format `lower-upper`!").ConfigureAwait(false); return;
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
            System.Threading.Thread.Sleep(0);

            await startGame(bottomFreq, topFreq, kanjis, fontFamily, ctx, answerTime);

            await ctx.Channel.SendMessageAsync($"Game finished!").ConfigureAwait(false);
        }


        private async Task startGame(int bottomFreq, int topFreq, List<kanji> kanjis, FontFamily fontFamily, CommandContext ctx, int answerTime = 10000) 
        {
            int streak = 0;
            List<int> pickedFreqs = new List<int>();
            if (topFreq > kanjis.Count - 1) { topFreq = kanjis.Count - 1; }
            var random = new Random((int)DateTime.UtcNow.Ticks);
            while (streak != -2)
            {
                pickFreq:
                int pickedFreq = -1;
                try
                {
                    pickedFreq = random.Next(bottomFreq, topFreq + 1);
                } catch { goto pickFreq; }
                
                foreach (int freq in pickedFreqs)
                {
                    if (pickedFreqs.Count >= topFreq - bottomFreq + 1)
                    { await ctx.Channel.SendMessageAsync("You have gone through all the kanji!").ConfigureAwait(false); goto endGame; }
                    else if (freq == pickedFreq) { goto pickFreq; }
                }

                pickedFreqs.Add(pickedFreq);
                string kanji = kanjis[pickedFreq].name;

                Font newFont = new Font(
                fontFamily,
                80,
                FontStyle.Regular,
                GraphicsUnit.Pixel);

                Image generatedImage = drawText(kanji, newFont, Color.Black, Color.White);
                try
                {
                    generatedImage.Save("KanjiImage.jpg");
                }
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

                InteractivityResult<DiscordMessage> result = await ctx.Channel.GetNextMessageAsync(
                    message => message.Content.Contains(kanji) == true || message.Content == "!stop" || message.Content == "!skip", TimeSpan.FromMilliseconds(answerTime))
                .ConfigureAwait(false);

                if (result.Result != null)
                {
                    if (result.Result.Content == "!stop") 
                    { streak = -1; return; }
                    if (result.Result.Content == "!skip") 
                    {
                        if (pickedFreqs.Count >= topFreq - bottomFreq + 1)
                        { 
                            await ctx.Channel.SendMessageAsync("You have gone through all the kanji!").ConfigureAwait(false); goto endGame;
                        }
                        else
                        {
                            streak = -1; continue;
                        }
                    }
                    
                    if (streak >= 0)
                    {
                        streak++;
                    }
                    else
                    {
                        streak = 1;
                    }
                    await ctx.Channel.SendMessageAsync($"**{result.Result.Author.Username}** got it right, streak is now {streak}!\n{kanji} has a frequency of {kanjis[pickedFreq].frequency}").ConfigureAwait(false);
                }
                else
                {
                    await ctx.Channel.SendMessageAsync($"No one got it right, the answer was {kanji} which has a frequency of {kanjis[pickedFreq].frequency}").ConfigureAwait(false);
                    System.Threading.Thread.Sleep(1000);
                    if (pickedFreqs.Count >= topFreq - bottomFreq + 1)
                    { await ctx.Channel.SendMessageAsync("You have gone through all the kanji!").ConfigureAwait(false); goto endGame; }
                    if (streak > 1)
                    {
                        streak = -1;
                    }
                    else
                    {
                        streak -= 1;
                    }
                }

                if (pickedFreqs.Count >= topFreq - bottomFreq + 1)
                { await ctx.Channel.SendMessageAsync("You have gone through all the kanji!").ConfigureAwait(false); goto endGame; }
                if (streak != -2)
                {
                    System.Threading.Thread.Sleep(3000);
                }
            }

        endGame:
            if (streak == -1) { streak = 0; }
            var gameEmbed = new DiscordEmbedBuilder()
            {
                Title = "Game finished",
                Description = streak == -2 ? "Streak: no streak" : $"Streak: {streak}",
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


        [Command("grabkanji")]
        [Hidden]
        public async Task grabKanji(CommandContext ctx)
        {
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





