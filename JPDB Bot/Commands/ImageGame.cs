//using DSharpPlus.CommandsNext;
//using DSharpPlus.CommandsNext.Attributes;
//using DSharpPlus.Entities;
//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using System.IO;
//using JPDB_Bot;
//using System.Drawing;
//using Image = System.Drawing.Image;
//using DSharpPlus.Interactivity.Extensions;
//using DSharpPlus.Interactivity;
//using System.Net;

//namespace JPDB_Bot.Commands
//{
//    public class TextGame : BaseCommandModule
//    {
//        //dependencies
//        private ConfigJson BotConfig;

//        private CommandContext ctx;


//        [Command("textgame")]
//        public async Task textGame()
//        {
//            string wordsFromServer = await GetWordsFromServer();

//            InteractivityResult<DiscordMessage> result = await ctx.Channel.GetNextMessageAsync(message =>
//            {
//                if (message.Content == "test")
//                {
//                    return true;
//                }
//                else
//                {
//                    return false;
//                }
//            }
//            ).ConfigureAwait(false);

//            Program.printMessage("Done text game!");
//        }




//        public Image DrawText(String text, Font font, Color textColor, Color backColor)
//        {
//            //first, create a dummy bitmap just to get a graphics object
//            Image img = new Bitmap(1, 1);
//            Graphics drawing = Graphics.FromImage(img);

//            //measure the string to see how big the image needs to be
//            SizeF textSize = drawing.MeasureString(text, font);

//            //free up the dummy image and old graphics object
//            img.Dispose();
//            drawing.Dispose();

//            //create a new image of the right size
//            img = new Bitmap((int)textSize.Width, (int)textSize.Height);

//            drawing = Graphics.FromImage(img);

//            //paint the background
//            drawing.Clear(backColor);

//            //create a brush for the text
//            Brush textBrush = new SolidBrush(textColor);

//            drawing.DrawString(text, font, textBrush, 0, 0);

//            drawing.Save();

//            textBrush.Dispose();
//            drawing.Dispose();

//            return img;

//        }


//        private async Task<string> GetWordsFromServer()
//        {
//            // pick_words?count=2&spread=100&users=user1,user2,user3
//            string url = "https://jpdb.io/api/experimental/pick_words?count=2&spread=300";

//            WebRequest request = WebRequest.Create(url);
//            Program.printAPIUse("Freqgame", url);

//            request.Method = "GET";
//            request.Headers["Authorization"] = "Bearer " + BotConfig.JPDBToken;

//            WebResponse response;
//            try
//            {
//                response = await request.GetResponseAsync();
//            }
//            catch (Exception e)
//            {
//                await ctx.Channel.SendMessageAsync(
//                        "API request failed, this is usually because of an incorrect jpdb username.\nThe game has been aborted.")
//                    .ConfigureAwait(false);
//                Console.WriteLine(e.Message);
//                Console.WriteLine("url is " + url);
//            }

//            //Console.WriteLine(response.StatusDescription);

//            //Get the stream containing content returned by the server.
//            Stream dataStream = response.GetResponseStream();
//            //Open the stream using a StreamReader for easy access.
//            StreamReader reader = new StreamReader(dataStream);
//            //Read the content.
//            string responseFromServer = await reader.ReadToEndAsync();
//            //Display the content.
//            Console.WriteLine(responseFromServer);
//            //Clean up the streams and the response.
//            reader.Close();
//            response.Close();

//            return responseFromServer;
//        }
//    }
//}