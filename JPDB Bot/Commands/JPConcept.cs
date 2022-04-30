using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net;
using System.Drawing;
using System.IO;
using DSharpPlus.Entities;

namespace JPDB_Bot.Commands
{
    public class JPConcept : BaseCommandModule
    {
        [Command("jpconcept")]
        [Cooldown(1, 2, CooldownBucketType.User)]
        [Description("Creates a Japanese concept meme")]
        public async Task jpConcept(CommandContext ctx, [Description("Word in Japanese")] string japanese = "食物", [Description("Romanised Word")] string romanised = "Tabemono", [Description("Meaning in English")] string meaning = "food", [Description("Yellow circle description")] string yellow = "Things that give you nutrition", [Description("Blue circle description")] string blue = "Things you put in your mouth")
        {
            Program.printCommandUse(ctx.User.Username, ctx.Message.Content);
            try
            {
                await generateImage(ctx, japanese, romanised, meaning, yellow, blue);
            } catch (Exception ex)
            {
                await ctx.RespondAsync("Something went wrong").ConfigureAwait(false);
                Program.printError("Tried to create a jpconcept but failed - " + ex.Message);
            }
        }

        private async Task generateImage(CommandContext ctx, string japanese, string romanised, string meaning, string yellow, string blue)
        {
            string imageFilePath = "jpconcept.png";
            Bitmap bitmap = (Bitmap)Image.FromFile(imageFilePath); //load the image file

            int meaningSize = 12;
            int jpSize = 15;
            int descSize = 12;
            if (meaning.Length > 50) { meaningSize = 7; }
            if (japanese.Length > 6) { jpSize = 10; }
            if (yellow.Length > 35 || blue.Length > 35) { descSize = 9; }

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                //image resolution: 586x586

                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Center;
                stringFormat.LineAlignment = StringAlignment.Center;

                graphics.DrawString(romanised, new Font("Segoe UI Black", 22), Brushes.Black, new Rectangle(0, 60, 586, 50), stringFormat); //160, 20, 250, 50
                graphics.DrawString($"A Japanese concept meaning", new Font("MV Boli", 12), Brushes.Black, new Rectangle(293 - 150, 115, 300, 25), stringFormat); //135, 60, 300, 50
                graphics.DrawString($"\"{meaning}\"", new Font("MV Boli", meaningSize), Brushes.Black, new Rectangle(0, 140, 586, 27), stringFormat); //135, 60, 300, 50

                graphics.DrawString(japanese, new Font("BIZ UDPゴシック", jpSize), Brushes.Black, new Rectangle(273, 210, 45, 160), stringFormat);

                graphics.DrawString(yellow, new Font("MV Boli", descSize), Brushes.Black, new Rectangle(107, 210, 134, 160), stringFormat);
                graphics.DrawString(blue, new Font("MV Boli", descSize), Brushes.Black, new Rectangle(346, 210, 124, 160), stringFormat);
            }

            try
            {
                bitmap.Save("NewConcept.png");
            } catch (Exception ex)
            {
                Program.printError($"Something went wrong when saving a jpconcept - {ex.Message}");
                await ctx.RespondAsync("Something went wrong").ConfigureAwait(false);
                return;
            }

            var fileStream = File.OpenRead("NewConcept.png");

            DiscordMessageBuilder discordMessageBuilder = new DiscordMessageBuilder();
            discordMessageBuilder.WithFile(fileStream);
            await discordMessageBuilder.SendAsync(ctx.Channel);

            fileStream.Dispose();
        }
    }

}