using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Threading.Tasks;

namespace JPDB_Bot.ContentRequests
{
    public class contentRequests : BaseCommandModule
    {
        public static DiscordMessage crMessage = null;
        public static DateTime lastCrMessage = new DateTime();


        [Command("cr")]
        [Cooldown(1, 20, CooldownBucketType.User)]
        [Hidden]
        [Description("Shows rule 6 for people that haven't read the rules :harold:")]
        public async Task crCommand(CommandContext ctx, string parameters)
        {
            var senderID = ctx.Message.Author.Id;
            if (senderID != 630381088404930560 && senderID != 245371520174522368 && senderID != 399993082806009856 && senderID != 118408957416046593) { return; }

            parameters = parameters.Trim().ToLower();
            if (parameters == "start")
            {
                var crChannel = ctx.Guild.GetChannel(980505150676418660);
                crMessage = await crChannel.SendMessageAsync("**Make sure to read pinned messages before requesting content!**");
                lastCrMessage = DateTime.Now;
                Program.printMessage("cr started succesfully");
            }
            else if (parameters == "stop")
            {
                crMessage = null;
                Program.printMessage("cr stopped succesfully");
            }
            return;
        }

        public static void crMessageSent(DiscordClient sender, MessageCreateEventArgs e)
        {
            TimeSpan minutesPast = crMessage == null ? new TimeSpan() : DateTime.Now - lastCrMessage;
            if (crMessage != null && !e.Author.IsBot & minutesPast.Minutes >= 30)
            {
                Program.printMessage("cr message about to trigger");
                var crChannel = e.Guild.GetChannel(980505150676418660);
                System.Threading.Thread.Sleep(58500);

                crMessage.DeleteAsync();
                crChannel.TriggerTypingAsync();
                System.Threading.Thread.Sleep(1500);

                crMessage = crChannel.SendMessageAsync("**Make sure to read pinned messages before requesting content!**").Result;

                lastCrMessage = DateTime.Now;
            }


        }
    }

}