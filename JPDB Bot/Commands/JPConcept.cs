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
using DSharpPlus.SlashCommands;

namespace JPDB_Bot.Commands
{
    public class JPConcept : ApplicationCommandModule
    {
        [SlashCommand("jpconcept", "Generate a meme using a set venn diagram template")]
        [Cooldown(1, 2, CooldownBucketType.User)]
        [Description("Creates a Japanese concept meme")]
        public async Task jpConcept(InteractionContext ctx, [Option("Japanese", "The word in Japanese")] string japanese, [Option("Romanised", "The word romanised")] string romanised, [Option("English", "An English translation of the word")] string meaning, [Option("Yellow", "The word to appear in the yellow circle")] string yellow, [Option("Blue", "The word to appear in the blue circle")] string blue)
        {
            Program.printCommandUse(ctx.User.Username, ctx.ToString());
            await ctx.CreateResponseAsync("Generating meme...").ConfigureAwait(false);
            try
            {
                await generateImage(ctx, japanese, romanised, meaning, yellow, blue);
            } catch (Exception ex)
            {
                Program.printError("Tried to create a jpconcept but failed - " + ex.Message);
            }
        }

        private async Task generateImage(InteractionContext ctx, string japanese, string romanised, string meaning, string yellow, string blue)
        {
            var webHook = new DiscordWebhookBuilder();

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

                graphics.DrawString(japanese, new Font("BIZ UDPÉSÉVÉbÉN", jpSize), Brushes.Black, new Rectangle(273, 210, 45, 160), stringFormat);

                graphics.DrawString(yellow, new Font("MV Boli", descSize), Brushes.Black, new Rectangle(107, 210, 134, 160), stringFormat);
                graphics.DrawString(blue, new Font("MV Boli", descSize), Brushes.Black, new Rectangle(346, 210, 124, 160), stringFormat);
            }

            try
            {
                bitmap.Save("NewConcept.png");
            } catch (Exception ex)
            {
                Program.printError($"Something went wrong when saving a jpconcept - {ex.Message}");
                webHook = new DiscordWebhookBuilder();
                webHook.WithContent("Something went wrong.");
                await ctx.EditResponseAsync(webHook);
                return;
            }

            var fileStream = File.OpenRead("NewConcept.png");

            DiscordMessageBuilder discordMessageBuilder = new DiscordMessageBuilder();
            discordMessageBuilder.WithFile(fileStream);

            try
            {
                await discordMessageBuilder.SendAsync(ctx.Channel);
                await ctx.DeleteResponseAsync();
            } catch
            {
                webHook = new DiscordWebhookBuilder();
                webHook.WithContent("Couldn't send the image.");
                await ctx.EditResponseAsync(webHook);
            } finally
            {
                fileStream.Dispose();
            }
            
        }
    }

}