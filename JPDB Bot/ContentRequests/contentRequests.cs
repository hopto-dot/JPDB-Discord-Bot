using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Threading.Tasks;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity.Enums;

namespace JPDB_Bot.ContentRequests
{
    public class contentRequests : BaseCommandModule
    {
        public static DiscordMessage crMessage = null;
        public static DateTime lastCrMessage = new DateTime();

        public static DiscordMessage fbMessage = null;
        public static DateTime LastFbMessage = new DateTime();

        [Command("pin")]
        [Aliases("pins", "cr")]
        [Description("Start pinning messages")]
        [Cooldown(3, 10, CooldownBucketType.User)]
        public async Task pinCommand(CommandContext ctx, string parameters = "")
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
                crMessage = await crChannel.SendMessageAsync("**Pinned message:\n\nPlease make sure to read the pinned messages before requesting content.**\n<https://discord.com/channels/799891866924875786/980505150676418660/980508358303948891>");
                fbMessage = await fbChannel.SendMessageAsync("**Pinned message:**\n\nPlease post __misparses, no audio, wrong audio, bad bilingual sentence and bad bilingual sentence translation__ issues on our GitHub issue tracker available here:\n<https://github.com/jpdb-io/issue-tracker/issues/new/choose>");

                await crMessage.ModifyEmbedSuppressionAsync(true);
                await fbMessage.ModifyEmbedSuppressionAsync(true);
                try
                {
                    lastCrMessage = DateTime.Now;
                    LastFbMessage = DateTime.Now;

                    Program.printMessage("pins started succesfully");
                } catch (Exception ex)
                {
                    Program.printError(ex.Message);
                    Program.printMessage("pins start: couldn't set LastMessage time to DateTime.Now");
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

            if (crMessage != null && !e.Author.IsBot)// && minutesPast.Minutes >= 2)
            {
                Program.printMessage("cr message about to trigger");
                var crChannel = e.Guild.GetChannel(980505150676418660);
                try
                {
                    crMessage.DeleteAsync();
                    crMessage = null;
                    System.Threading.Thread.Sleep(50);

                    crMessage = crChannel.SendMessageAsync("**Pinned message:\n\nPlease make sure to read the pinned messages before requesting content.**\n<https://discord.com/channels/799891866924875786/980505150676418660/980508358303948891>").Result;
                    crMessage.ModifyEmbedSuppressionAsync(true);

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

            if (fbMessage != null && !e.Author.IsBot)// && minutesPast.Minutes >= 2)
            {
                Program.printMessage("fb message about to trigger");
                var fbChannel = e.Guild.GetChannel(850630303403343883);
                try
                {
                    fbMessage.DeleteAsync();
                    fbMessage = null;
                    System.Threading.Thread.Sleep(50);

                    fbMessage = fbChannel.SendMessageAsync("**Pinned message:**\n\nFrom now on please post __misparses, no audio, wrong audio, bad bilingual sentence and bad bilingual sentence translation__ or deck issues such as __duplicate entries in the database / wrong covers / wrong MAL links etc__ on our GitHub issue tracker available here:\n<https://github.com/jpdb-io/issue-tracker/issues/new/choose>").Result;
                    fbMessage.ModifyEmbedSuppressionAsync(true);

                    LastFbMessage = DateTime.Now;
                    Program.printMessage("cr message successfully triggered");
                }
                catch
                {
                    Program.printError("cr message triggering went wrong");
                }
            }

            string msgContent = e.Message.Content.ToLower().Trim();
            if (msgContent.Contains("https://jpdb.io/") && e.Author.IsBot == false && e.Author.Id != 118408957416046593 && !msgContent.Contains("label") && !msgContent.Contains("git") && !msgContent.Contains("category") && !msgContent.Contains("kanji") && !msgContent.Contains("stroke"))
            {
                string[] disallowedWords = { "misparse", "sentence", "cover", "audio", "duplicate", "wrong", "same", "bad translation", "bad sentence", "bad bilingual", "identical" };
                bool triggered = false;
                string triggerWord = "";
                foreach (string word in disallowedWords)
                {
                    if (msgContent.Contains(word))
                    {
                        triggerWord = word;
                        triggered = true;
                    }
                }

                if (triggered == true)
                {
                    Program.printMessage($"{e.Author.Username} triggered a issue redirect with the word {triggerWord}.");
                    e.Guild.GetMemberAsync(e.Author.Id).Result.SendMessageAsync("Hello,\n\nYou triggered this message because it seems you reported an issue that should be reported at the link below:\n<https://github.com/jpdb-io/issue-tracker/issues/new/choose>\n\nAs a reminder, the following should be reported on GitHub: misparses, no audio, wrong audio, bad bilingual sentence and bad bilingual sentence translation__ or deck issues such as __duplicate entries in the database, wrong covers or wrong MAL links__");
                }
            }

        }
    }
}