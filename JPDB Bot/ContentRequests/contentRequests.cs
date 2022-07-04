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

        public static DiscordMessage fbMessage = null;
        public static DateTime LastFbMessage = new DateTime();

        [Command("pin")]
        [Hidden]
        public async Task crCommand(CommandContext ctx, string parameters)
        {
            var senderID = ctx.Message.Author.Id;
            if (senderID != 630381088404930560 && senderID != 245371520174522368 && senderID != 399993082806009856 && senderID != 118408957416046593)
            {
                try
                {
                    await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":dogegun:"));
                }
                catch { }
                return;
            }

            parameters = parameters.Trim().ToLower();
            if (parameters == "start")
            {
                var crChannel = ctx.Guild.GetChannel(980505150676418660);
                var fbChannel = ctx.Guild.GetChannel(850630303403343883);
                if (crChannel == null || fbChannel == null)
                {
                    return;
                }
                crMessage = await crChannel.SendMessageAsync("**Pinned message:\n\nPlease make sure to read the pinned messages before requesting content.**\nhttps://discord.com/channels/799891866924875786/980505150676418660/980508358303948891");
                fbMessage = await fbChannel.SendMessageAsync("**Pinned message:**\n\nPlease post __misparses, no audio, wrong audio, bad bilingual sentence and bad bilingual sentence translation__ issues on our GitHub issue tracker available here:\nhttps://github.com/jpdb-io/issue-tracker/issues/new/choose");
                try
                {
                    lastCrMessage = DateTime.Now;
                    LastFbMessage = DateTime.Now;

                    Program.printMessage("cr started succesfully");
                } catch (Exception ex)
                {
                    Program.printError(ex.Message);
                    Program.printMessage("cr start: couldn't set lastCrMessage time to now");
                }
            }
            else if (parameters == "stop")
            {
                crMessage = null;
                fbMessage = null;
                Program.printMessage("cr stopped succesfully");
            }
            return;
        }

        public static void crMessageSent(DiscordClient sender, MessageCreateEventArgs e)
        {
            TimeSpan minutesPast = new TimeSpan();
            try
            {
                minutesPast = crMessage == null ? new TimeSpan() : DateTime.Now - lastCrMessage;
            } catch { Program.printMessage("cr message - time span calculation failed"); return; }
            Program.printMessage($"cr time difference: {minutesPast.Minutes}");

            if (crMessage != null && !e.Author.IsBot & minutesPast.Minutes >= 2)
            {
                Program.printMessage("cr message about to trigger");
                var crChannel = e.Guild.GetChannel(980505150676418660);
                try
                {
                    crMessage.DeleteAsync();
                    crMessage = null;
                    System.Threading.Thread.Sleep(50);

                    crMessage = crChannel.SendMessageAsync("**Pinned message:\n\nPlease make sure to read the pinned messages before requesting content.**\nhttps://discord.com/channels/799891866924875786/980505150676418660/980508358303948891").Result;

                    lastCrMessage = DateTime.Now;
                    Program.printMessage("cr message successfully triggered");
                } catch
                {
                    Program.printError("cr message triggering went wrong");
                }
            }


        }

        public static void fbMessageSent(DiscordClient sender, MessageCreateEventArgs e)
        {
            TimeSpan minutesPast = new TimeSpan();
            try
            {
                minutesPast = fbMessage == null ? new TimeSpan() : DateTime.Now - LastFbMessage;
            }
            catch { Program.printMessage("fb message - time span calculation failed"); return; }
            Program.printMessage($"fb time difference: {minutesPast.Minutes}");

            if (fbMessage != null && !e.Author.IsBot & minutesPast.Minutes >= 2)
            {
                Program.printMessage("fb message about to trigger");
                var fbChannel = e.Guild.GetChannel(850630303403343883);
                try
                {
                    fbMessage.DeleteAsync();
                    fbMessage = null;
                    System.Threading.Thread.Sleep(50);

                    fbMessage = fbChannel.SendMessageAsync("**Pinned message:**\n\nFrom now on please post __misparses, no audio, wrong audio, bad bilingual sentence and bad bilingual sentence translation__ issues on our GitHub issue tracker available here:\nhttps://github.com/jpdb-io/issue-tracker/issues/new/choose").Result;

                    LastFbMessage = DateTime.Now;
                    Program.printMessage("cr message successfully triggered");
                }
                catch
                {
                    Program.printError("cr message triggering went wrong");
                }
            }


        }
    }

}