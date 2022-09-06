using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using JPDB_Bot;
using System.Net;

namespace JPDB_Bot.Commands
{
    public class RoleCommand : BaseCommandModule
    {

        [Command("role")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        [Description("Assign your server a role:\nBumper - get notified when you can use the \"!d bump\" command")]
        public async Task role(CommandContext ctx, string role, string add)
        {
            Program.printCommandUse(ctx.Message.Author.Username, ctx.Message.Content);
            var jpdbRoles = Bot.jpdbGuild.Roles;

            ulong roleid = 0;
            if (role == "bumper" || role == "bump") { roleid = 903316747472474193; }
            if (role == "quietstudy" || role == "quiet") { roleid = 934783869104828466; }
            if (roleid == 0) { return; }

            DiscordMember roleUser = ctx.Guild.GetMemberAsync(ctx.Message.Author.Id).Result;
            DiscordRole giveRole = ctx.Guild.GetRole(roleid);

            if (add == "assign" || add == "give" ||add == "add" || add == "enroll" || add == "true" || add == "yes" || add == "become" || add == "subscribe")
            {
                try {
                    await roleUser.GrantRoleAsync(giveRole);
                    await ctx.RespondAsync($"You now have the {role} role.");
                } catch { return; }
                
            }
            if (add == "remove" || add == "retire" || add == "false" || add == "no" || add == "stop" || add == "unsubscribe")
            {
                try {
                    await roleUser.RevokeRoleAsync(giveRole);
                    await ctx.RespondAsync($"You no longer have the {role} role.");
                } catch (Exception ex) { Console.WriteLine(ex.Message);  return; }
                
            }
        }

        [Command("message")]
        [Hidden]
        public async Task message(CommandContext ctx, string channelID, [RemainingText] string message)
        {
            if (ctx.Message.Author.Id != 630381088404930560) { return; }
            try
            {
                await ctx.Client.GetChannelAsync(ulong.Parse(channelID)).Result.TriggerTypingAsync();
                System.Threading.Thread.Sleep(2000);
                await ctx.Client.GetChannelAsync(ulong.Parse(channelID)).Result.SendMessageAsync(message.Replace("\\n", "\n"));
            } catch
            {
                Program.printMessage($"Failed to send '{message}' to '{channelID}'");
            }
        }

        [Command("react")]
        [Hidden]
        public async Task react(CommandContext ctx, string channelID, string messageID, string emoji)
        {
            if (ctx.Message.Author.Id != 630381088404930560) { return; }
            try
            {
                var channelResult = await ctx.Client.GetChannelAsync(ulong.Parse(channelID));
                var messageReslt = await channelResult.GetMessageAsync(ulong.Parse(messageID));
                await messageReslt.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, $":{emoji}:"));
            }
            catch
            {
                Program.printMessage($"Failed to react with ':{emoji}:' to message '{messageID}' in channel '{channelID}'");
            }
        }

        [Command("reply")]
        [Hidden]
        public async Task reply(CommandContext ctx, string channelID, string messageID, [RemainingText] string message)
        {
            if (ctx.Message.Author.Id != 630381088404930560) { return; }
            try
            {
                await ctx.Client.GetChannelAsync(ulong.Parse(channelID)).Result.TriggerTypingAsync();
                System.Threading.Thread.Sleep(2000);
                var channelResult = await ctx.Client.GetChannelAsync(ulong.Parse(channelID));
                var messageReslt = await channelResult.GetMessageAsync(ulong.Parse(messageID));
                await messageReslt.RespondAsync(message.Replace("\\n", "\n"));
            }
            catch
            {
                Program.printMessage($"Failed to respond with ':{message}:' to message '{messageID}' in channel '{channelID}'");
            }
        }

        [Command("image")]
        [Hidden]
        public async Task image(CommandContext ctx, string channelID, [RemainingText] string message = "")
        {
            if (ctx.Message.Author.Id != 630381088404930560) { return; }
            try
            {
                if (ctx.Message.Attachments.Count == 0) { return; }
                string attachmentURL = ctx.Message.Attachments[0].Url;

                if (message.Trim() != "")
                {
                    await ctx.Client.GetChannelAsync(ulong.Parse(channelID)).Result.SendMessageAsync(message.Replace("\\n", "\n"));
                }

                var wc = new WebClient();
                wc.DownloadFile(attachmentURL, "image.jpg");

                //await ctx.Client.GetChannelAsync(ulong.Parse(channelID)).Result.SendMessageAsync(message.Replace("\\n", "\n") + $"\n\n{attachmentURL}");
                var fileStream = File.OpenRead("image.jpg");

                DiscordMessageBuilder discordMessageBuilder = new DiscordMessageBuilder();
                discordMessageBuilder.WithFile(fileStream);
                await discordMessageBuilder.SendAsync(ctx.Client.GetChannelAsync(ulong.Parse(channelID)).Result);

                fileStream.Dispose();
                File.Delete("image.jpg");
            }
            catch
            {
                Program.printMessage($"Failed to image message in channel '{channelID}'");
            }
        }
    }
}