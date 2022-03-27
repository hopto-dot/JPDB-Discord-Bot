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
            //PointF firstLocation = new PointF(100f, 30f);
            //PointF secondLocation = new PointF(10f, 50f);

            string imageFilePath = "jpconcept.png";
            Bitmap bitmap = (Bitmap)Image.FromFile(imageFilePath); //load the image file

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                //total 586x586

                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Center;
                stringFormat.LineAlignment = StringAlignment.Center;

                // Draw the text and the surrounding rectangle.
                graphics.DrawString(romanised, new Font("Segoe UI Black", 22), Brushes.Black, new Rectangle(0, 60, 586, 50), stringFormat); //160, 20, 250, 50
                graphics.DrawString($"A Japanese concept meaning \"{meaning}\"", new Font("MV Boli", 12), Brushes.Black, new Rectangle(293 - 150, 115, 300, 50), stringFormat); //135, 60, 300, 50

                graphics.DrawString(japanese, new Font("BIZ UDPゴシック", 15), Brushes.Black, new Rectangle(273, 212, 45, 160), stringFormat);

                graphics.DrawString(yellow, new Font("MV Boli", 15), Brushes.Black, new Rectangle(118, 210, 110, 160), stringFormat);
                graphics.DrawString(blue, new Font("MV Boli", 15), Brushes.Black, new Rectangle(358, 210, 110, 160), stringFormat);

            }

            

            bitmap.Save("NewConcept.png"); //save the image file

            var fileStream = File.OpenRead("NewConcept.png");

            DiscordMessageBuilder discordMessageBuilder = new DiscordMessageBuilder();
            discordMessageBuilder.WithFile(fileStream);
            await discordMessageBuilder.SendAsync(ctx.Channel);

            fileStream.Dispose();
        }
    }

}