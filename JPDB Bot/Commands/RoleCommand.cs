using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using JPDB_Bot;

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
    }
}