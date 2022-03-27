using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPDB_Bot.Guess_the_Kanji
{
    public class guessTheKanji : BaseCommandModule
    {
        [Command("genhandwriting")]
        [Cooldown(2, 2, CooldownBucketType.User)]
        [Description("Generate an image of the provide text in a handwriting font")]
        public async Task genHandwriting(CommandContext ctx, [RemainingText] string text)
        {
            if (text.Contains("<") && text.Contains(">"))
            {
                string fontString = snipBrackets(text);
                await runGenHandwriting(fontString, ctx, text.Replace("<" + fontString + ">", ""));
            }
            else
            {
                await runGenHandwriting("HG行書体", ctx, text);
            }
        }

        [Command("fonts")]
        [Cooldown(1, 10, CooldownBucketType.Channel)]
        [Description("Lists all installed fonts")]
        public async Task fonts(CommandContext ctx)
        {
            InstalledFontCollection fontCollection = new InstalledFontCollection();
            FontFamily[] fontFamilies = fontCollection.Families;
            
            string output = "";
            foreach (FontFamily fontFamily in fontFamilies)
            {
                output += fontFamily.Name + "\n";
            }
            //await ctx.RespondAsync("```\n" + output + "\n```").ConfigureAwait(false);

            File.WriteAllText("fonts.txt", output);

            FileStream fsSource = new FileStream("fonts.txt",
            FileMode.Open, FileAccess.Read);

            DiscordMessageBuilder discordMessageBuilder = new DiscordMessageBuilder();
            discordMessageBuilder.WithFile("fonts.txt", fsSource);
            await discordMessageBuilder.SendAsync(ctx.Channel);

        }

        private async Task runGenHandwriting(string fontName, CommandContext ctx, [RemainingText] string text)
        {
            FontFamily fontFamily; //"みつフォント", "青柳疎石フォント2 OTF", "HG行書体", "UD Digi Kyokasho N-R"
            try
            {
                fontFamily = new FontFamily(fontName); //
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

            Font newFont = new Font(
               fontFamily,
               80,
               FontStyle.Regular,
               GraphicsUnit.Pixel);

            //fontFamily = new FontFamily("C:\\Users\\George\\source\\repos\\JPDB-Discord-Bot\\JPDB Bot\\bin\\Debug\net5.0\\AoyagiSosekiFont2.otf");

            Image generatedImage = drawText(text, newFont, Color.Black, Color.White);

            //try { File.Delete("KanjiImage.jpg"); } catch { }

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
        }

        private string snipBrackets(string input)
        {
            return input.Split('<', '>')[1];
        }

        public Image drawText(String text, Font font, Color textColor, Color backColor)
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
