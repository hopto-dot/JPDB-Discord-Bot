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
        public static DiscordMessage fbMessage = null;

        [Command("pin")]
        [Aliases("pins", "cr")]
        [Hidden]
        [Description("Start pinning messages")]
        [Cooldown(3, 10, CooldownBucketType.User)]
        public async Task pinCommand(CommandContext ctx, [RemainingText] string parameters = "")
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
            if (parameters == "start" || parameters == "")
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
                Program.printMessage("pins started succesfully");
            }
            else if (parameters == "stop")
            {
                crMessage = null;
                fbMessage = null;
                Program.printMessage("cr stopped succesfully");
            }
            return;
        }

        public async static void crMessageSent(DiscordClient sender, MessageCreateEventArgs e)
        {
            if (crMessage != null && !e.Author.IsBot)
            {
                Program.printMessage("cr message about to trigger");
                var crChannel = e.Guild.GetChannel(980505150676418660);
                try
                {
                    await crMessage.DeleteAsync();
                    crMessage = null;
                    System.Threading.Thread.Sleep(50);

                    crMessage = await crChannel.SendMessageAsync("**Pinned message:\n\nPlease make sure to read the pinned messages before requesting content.**\n<https://discord.com/channels/799891866924875786/980505150676418660/980508358303948891>");
                    await crMessage.ModifyEmbedSuppressionAsync(true);

                    Program.printMessage("cr message successfully triggered");
                } catch
                {
                    Program.printError("cr message triggering went wrong");
                }
            }
            else if (crMessage == null)
            {
                Program.printError($"(crMessageSent) crMessage is null");
            }


        }

        public async static void fbMessageSent(DiscordClient sender, MessageCreateEventArgs e)
        {
            if (fbMessage != null && !e.Author.IsBot)
            {
                Program.printMessage("fb message about to trigger");
                var fbChannel = e.Guild.GetChannel(850630303403343883);
                try
                {
                    await fbMessage.DeleteAsync();
                    fbMessage = null;
                    System.Threading.Thread.Sleep(50);

                    fbMessage = await fbChannel.SendMessageAsync("**Pinned message:**\n\nFrom now on please post __misparses, no audio, wrong audio, bad bilingual sentence and bad bilingual sentence translation__ or deck issues such as __duplicate entries in the database / wrong covers / wrong MAL links etc__ on our GitHub issue tracker available here:\n<https://github.com/jpdb-io/issue-tracker/issues/new/choose>");

                    Program.printMessage("cr message successfully triggered");
                }
                catch
                {
                    Program.printError("cr message triggering went wrong");
                }
            }
            else if (fbMessage == null)
            {
                Program.printError($"(crMessageSent) crMessage is null");
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
                    await e.Guild.GetMemberAsync(e.Author.Id).Result.SendMessageAsync("Hello,\n\nYou triggered this message because it seems you reported an issue that should be reported at the link below:\n<https://github.com/jpdb-io/issue-tracker/issues/new/choose>\n\nAs a reminder, the following should be reported on GitHub: misparses, no audio, wrong audio, bad bilingual sentence and bad bilingual sentence translation__ or deck issues such as __duplicate entries in the database, wrong covers or wrong MAL links__");
                }
            }

        }
    }
}