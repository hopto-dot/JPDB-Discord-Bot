using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Entities;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DiscordBot.Commands
{
    public class TestCommands : BaseCommandModule
    {
        [Command("hi")]
        [Cooldown(2, 10, CooldownBucketType.User)]
        [Description("Get a nice (or bad) response")]
        public async Task hi(CommandContext ctx)
        {
            Program.PrintCommandUse(ctx.User.Username, ctx.Message.Content);
            Random random = new Random();
            int randomInt = random.Next(1, 16);
            string Output = string.Empty;
            if (ctx.Member.Roles.Any(r => r.Name == "Owner" || r.Name == "Supporter" || r.Name == "Server Booster"))
            {
                switch (randomInt)
                {
                    case 1:
                        goto case 7;
                    case 2:
                        goto case 7;
                    case 3:
                        goto case 7;
                    case 4:
                        goto case 15;
                    case 5:
                        Output = "Hi " + ctx.User.Username + "様, I can speak English too ya know >:)";
                        break;
                    case 6:
                        goto case 11;
                    case 7:
                        Output = "どうも、" + ctx.User.Username + "様 :)";
                        break;
                    case 8:
                        Output = "どうも、" + ctx.User.Username + "様 ;)";
                        break;
                    case 9:
                        Output = "よおおおおおおおぉ " + ctx.User.Username + "様！ :)";
                        break;
                    case 10:
                        goto case 11;
                    case 11:
                        Output = "また会えて嬉しいね " + ctx.User.Username + "様 :)";
                        break;
                    case 12:
                        Output = "やっほおおおおおお～ " + ctx.User.Username + "様 :)";
                        break;
                    case 13:
                        Output = "おおおおっす! " + ctx.User.Username + "様 :)";
                        break;
                    case 14:
                        Output = "ハロオオオ " + ctx.User.Username + "様！ :)";
                        break;
                    case 15:
                        Output = "へっ！なんかあった？" + ctx.User.Username + "様 :O";
                        break;
                }
            }
            else
            {
                switch (randomInt)
                {
                    case 1:
                        goto case 7;
                    case 2:
                        goto case 7;
                    case 3:
                        goto case 7;
                    case 4:
                        goto case 11;
                    case 5:
                        goto case 12;
                    case 6:
                        goto case 14;
                    case 7:
                        Output = "どうも、" + ctx.User.Username + " :)";
                        break;
                    case 8:
                        Output = "どうも、" + ctx.User.Username + " ;)";
                        break;
                    case 9:
                        Output = "おいお前 JPDBの支援者になれ";
                        break;
                    case 10:
                        goto case 12;
                    case 11:
                        goto case 12;
                    case 12:
                        Output = "元気はないんだなあ " + ctx.User.Username + "さん。JPDBを支援したら？うwう";
                        break;
                    case 13:
                        Output = "おっす! " + ctx.User.Username + " :)";
                        break;
                    case 14:
                        Output = "ハロー " + ctx.User.Username + "！ :)";
                        break;
                    case 15:
                        Output = "へっ！なんかあった？" + ctx.User.Username + " :O";
                        break;
                }
            }
            await ctx.RespondAsync(Output).ConfigureAwait(false);
        }

        [Command("freqgame")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        [Description("Play a game where you guess which word is more frequent")]
        public async Task guessgame(CommandContext ctx, [DescriptionAttribute("Your jpdb username")] string startPlayer, [DescriptionAttribute("Easy|Medium|Hard|Extreme|Godmode (Medium by default)")] string difficulty = "medium")
        {
            Program.PrintCommandUse(ctx.User.Username, ctx.Message.Content);

            int minFreq; int maxFreq; double answerTime = 3;
            difficulty = difficulty.ToLower();
            switch (difficulty)
            {
                case "easy":
                    minFreq = 100;
                    maxFreq = 1500;
                    answerTime = 5;
                    difficulty = "Easy";
                    break;
                case "medium":
                    minFreq = 2000;
                    maxFreq = 3500;
                    answerTime = 5;
                    difficulty = "Medium";
                    break;
                case "hard":
                    minFreq = 5500;
                    maxFreq = 7000;
                    answerTime = 4.5;
                    difficulty = "Hard";
                    break;
                case "extreme":
                    minFreq = 7000;
                    maxFreq = 9000;
                    answerTime = 4;
                    difficulty = "Extreme";
                    break;
                case "godmode":
                    minFreq = 15000;
                    maxFreq = 25000;
                    answerTime = 3;
                    difficulty = "Godmode";
                    break;
                default:
                    goto case "medium";
            }

            DiscordUser Player1 = ctx.Message.Author;
            string User1 = startPlayer;

            await ctx.Channel.SendMessageAsync($"Type \"!participate [username]\" to play with {ctx.User.Username}").ConfigureAwait(false);

            DSharpPlus.Interactivity.InteractivityResult<DSharpPlus.Entities.DiscordMessage> result;
            try
            {
                do
                {
                    result = await ctx.Channel.GetNextMessageAsync(m =>
                    {
                        return (m.Content.ToLower()).Substring(0, 13) == "!participate ";
                    }).ConfigureAwait(false);
                    if (result.TimedOut)
                    {
                        await ctx.Channel.SendMessageAsync("Game timed out").ConfigureAwait(false);
                        return;
                    }
                    else if (result.Result.Author.Username == ctx.Message.Author.Username)
                    {
                        //await ctx.RespondAsync("You can't play against yourself lol").ConfigureAwait(false);
                    }

                } while (result.Result.Author.Username == ctx.Message.Author.Username && true == false);
            } catch
            {
                await ctx.Channel.SendMessageAsync("Game timed out.").ConfigureAwait(false);
                return;
            }
            
            DiscordUser Player2 = result.Result.Author;
            string User2 = result.Result.Content.Substring(13);
            //await ctx.Channel.SendMessageAsync(User2);
            //await ctx.RespondAsync($"{Player1.Username} ({User1}) 対 {Player2.Username} ({User2})").ConfigureAwait(false);
            var gameEmbed = new DiscordEmbedBuilder
            {
                Title = $"Guessing game ({difficulty})",
                Description = $"{Player1.Username} ({User1}) 対 {Player2.Username} ({User2})",
                Color = DiscordColor.Red,
                Footer = new DiscordEmbedBuilder.EmbedFooter {
                    Text = "Currently, reactions and usernames don't do anything.",
                }
            };
            await ctx.Channel.SendMessageAsync(embed: gameEmbed).ConfigureAwait(false);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Reading config file...");
            var json = string.Empty;

            try
            {
                using (var fs = File.OpenRead("config.json"))
                {
                    using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                    {
                        json = await sr.ReadToEndAsync().ConfigureAwait(false);

                    }
                }
            }
            catch
            {
                Program.PrintError("Couldn't read config.json");
                return;
            }

            ConfigJson configJson;
            try
            {
                configJson = JsonConvert.DeserializeObject<ConfigJson>(json);
            }
            catch
            {
                Program.PrintError("Couldn't deserialize config.json");
                return;
            }



            int p1Points = 0; int p2Points = 0;
            for (int round = 1; round <= 3; round ++)
            {
                
                WebRequest request;
                //request = WebRequest.Create("https://jpdb.io/api/experimental/pick_word_pair?rank_at_least=2000&rank_at_most=100&user_1=spectaku&user_2=alemax");
                request =  WebRequest.Create("https://jpdb.io/api/experimental/pick_word_pair?rank_at_least=" + maxFreq + "&rank_at_most=" + minFreq);
                //request.Credentials = CredentialCache.DefaultCredentials
                request.Method = "GET";
                request.Headers["Authorization"] = "Bearer " + configJson.JPDBToken;

                HttpWebResponse response = null;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                } catch (WebException e)
                {
                    //var eresponse = ((HttpWebResponse)e.Response);
                    //var someheader = response.Headers["X-API-ERROR"];
                    //// check header

                    //if (e.Status == WebExceptionStatus.ProtocolError)
                    //{
                    //    // protocol errors find the statuscode in the Response
                    //    // the enum statuscode can be cast to an int.
                    //    int code = (int)((HttpWebResponse)e.Response).StatusCode;
                    //    string content;
                    //    var ereader = new StreamReader(e.Response.GetResponseStream());
                    //    content = ereader.ReadToEnd();
                    //}
                    //// do what ever you want to store and return to your callers
                    return;
                }

                Console.WriteLine(response.StatusDescription);

                //Get the stream containing content returned by the server.
                Stream dataStream = response.GetResponseStream();
                //Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                //Read the content.
                String responseFromServer = reader.ReadToEnd();
                //Display the content.
                Console.WriteLine(responseFromServer);
                //Clean up the streams and the response.
                reader.Close();
                response.Close();
                //JsonArrayAttribute jsonResponse = JsonConvert.DeserializeObject<J>(responseFromServer);
                JObject jsonServerResponse = JObject.Parse(responseFromServer);
                Console.WriteLine("Parsed JSON response");

                await Task.Delay(1000);

                Newtonsoft.Json.Linq.JToken token1 = jsonServerResponse.GetValue("word_1");
                Newtonsoft.Json.Linq.JToken token2 = jsonServerResponse.GetValue("word_2");

                Vocabulary wordA = new Vocabulary
                {
                    vocabKanji = token1.SelectToken("spelling").ToString(),
                    vocabReading = token1.SelectToken("reading").ToString(),
                    vocabFreq = token1.SelectToken("rank").ToObject<int>(),
                    //vocabMeaning = token1.SelectToken("spelling").ToObject<String []>(),
                };
                Vocabulary wordB = new Vocabulary
                {
                    vocabKanji = token2.SelectToken("spelling").ToString(),
                    vocabReading = token2.SelectToken("reading").ToString(),
                    vocabFreq = token2.SelectToken("rank").ToObject<int>(),
                    //vocabMeaning = token2.SelectToken("spelling").ToObject<String[]>(),
                };
                Console.WriteLine("Parsed words.");


                ////////////////////////END OF API////////////////////////


                gameEmbed = new DiscordEmbedBuilder
                {
                    Title = "Which word is more frequent?",
                    Description = $"A = {wordA.vocabKanji} ({wordA.vocabReading})\nB = {wordB.vocabKanji} ({wordB.vocabReading})",
                    Color = DiscordColor.Red,
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"{Player1.Username}: {p1Points} points\n{Player2.Username}: {p2Points} points",
                    }
                };

                var interactivity = ctx.Client.GetInteractivity();
                var questionMessage = await ctx.Channel.SendMessageAsync(embed: gameEmbed).ConfigureAwait(false);

                await questionMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":regional_indicator_a:"));
                await questionMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":regional_indicator_b:"));

                //await Task.Delay(3000);

                var reactionResult = await interactivity.CollectReactionsAsync(questionMessage, TimeSpan.FromSeconds(answerTime));
                var distinctResult = reactionResult.ToArray();
                string player1Choice = "0"; string player2Choice = "0";

                foreach (var Reaction in distinctResult.ToArray())
                {
                    if (Reaction.Emoji.Name == "🇦")
                    {   
                        foreach (var User in Reaction.Users.ToArray()) //loop through all the users who've reacted with A
                        {
                            if (User.Username == Player1.Username) {
                                if (player1Choice != "0") //if player1 has voted on both options
                                {
                                    player1Choice = "-1";
                                }
                                else
                                {
                                    player1Choice = "A";
                                }
                            } 
                            else if (User.Username == Player2.Username)
                            {
                                if (player2Choice != "0") //if player2 has voted on both options
                                {
                                    player2Choice = "-1"; 
                                }
                                else
                                {
                                    player2Choice = "A";
                                }
                            }
                        }
                    }
                    else if (Reaction.Emoji.Name == "🇧") //loop through all the users who've reacted with A
                    {
                        foreach (var User in Reaction.Users.ToArray()) //loop through all the users who've reacted with A
                        {
                            if (User.Username == Player1.Username)
                            {
                                if (player1Choice != "0") //if player1 has voted on both options
                                {
                                    player1Choice = "-1";
                                }
                                else
                                {
                                    player1Choice = "B";
                                }
                            }
                            else if (User.Username == Player2.Username)
                            {
                                if (player2Choice != "0") //if player2 has voted on both options
                                {
                                    player2Choice = "-1";
                                }
                                else
                                {
                                    player2Choice = "B";
                                }
                            }
                        }
                    }
                }

                if (Math.Min(wordA.vocabFreq, wordB.vocabFreq) == wordA.vocabFreq)
                {
                    if (player1Choice == "A")
                    {
                        p1Points++;
                        await ctx.Channel.SendMessageAsync($"{Player1.Username} was correct!");
                    }
                    if (player2Choice == "A")
                    {
                        p2Points++;
                        await ctx.Channel.SendMessageAsync($"{Player2.Username} was correct!");
                    }
                    await ctx.Channel.SendMessageAsync($"The answer was A ({wordA.vocabKanji}) with a frequency of {wordA.vocabFreq} while {wordB.vocabKanji} was {wordB.vocabFreq}").ConfigureAwait(false);
                }
                else
                {
                    if (player1Choice == "B")
                    {
                        p1Points++;
                        await ctx.Channel.SendMessageAsync($"{Player1.Username} was correct!");
                    }
                    if (player2Choice == "B")
                    {
                        p2Points++;
                        await ctx.Channel.SendMessageAsync($"{Player2.Username} was correct!");
                    }
                    await ctx.Channel.SendMessageAsync($"The answer was B ({wordB.vocabKanji}) with a frequency of {wordB.vocabFreq} while {wordA.vocabKanji} was {wordA.vocabFreq}").ConfigureAwait(false);
                }

                await Task.Delay(4500);
            }
        }

        [Command("changelog")]
        [Cooldown(1, 20, CooldownBucketType.User)]
        [Description("Shows the latest addition to the changelog")]
        public async Task changeLog(CommandContext ctx)
        {
            Program.PrintCommandUse(ctx.User.Username, ctx.Message.Content);
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
        }

        [Command("content")]
        [Cooldown(3, 10, CooldownBucketType.User)]
        [Description("Search for content in the JPDB database\nFor example: !content \"steins gate\"")]
        public async Task content(CommandContext ctx, [DescriptionAttribute("Name of the content you are searching")] string searchString)


        {
            Program.PrintCommandUse(ctx.User.Username, ctx.Message.Content);
            //await ctx.Channel.SendMessageAsync("Searching for " + searchString + "...").ConfigureAwait(false);


            WebClient Client = new WebClient();
            Client.Encoding = System.Text.Encoding.UTF8;
            string HTML = "";
            int snipIndex = -1;

            string OriginalFilter = string.Empty;
            OriginalFilter = "anime";

            if (HTML.Length > 250) return;

            string wordTemp = "";
            string URL = "";
            List<string> wordIDs = new List<string>() { };
            URL = "https://jpdb.io/prebuilt_decks?q=" + searchString;
            try
            {
                HTML = Client.DownloadString(new Uri(URL));
            }
            catch (Exception ex)
            {
                Program.PrintError(ex.Message);
                return;
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
            if (wordTemp.Contains(">") == false && wordTemp.Contains("<") == false & wordTemp.Contains("=") == false & wordTemp.Contains("-") == false)
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

                    snipIndex = HTML.IndexOf("#a") + 3;
        }

        [Command("japantime")]
        [Cooldown(2, 10, CooldownBucketType.User)]
        [Description("Check the time in Japan")]
        public async Task japantime(CommandContext ctx)
        {
            Program.PrintCommandUse(ctx.User.Username, ctx.Message.Content);
            var info = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
            DateTimeOffset localServerTime = DateTimeOffset.Now;
            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);
            String TimeInJapan = localTime.ToString("dd/MM/yyyy HH:mm:ss");
            var Kou = await ctx.Client.GetUserAsync(118408957416046593);
            if ((localTime.Hour > 21 || localTime.Hour < 5) && Kou.Presence.Status != DSharpPlus.Entities.UserStatus.Offline)
            {
                await ctx.RespondAsync("日本: " + TimeInJapan + $"\n{Kou.Username} is up late working on JPDB for us all <3").ConfigureAwait(false);
            } else
            {
                await ctx.RespondAsync("日本: " + TimeInJapan).ConfigureAwait(false);
            }
        }
    }


}
