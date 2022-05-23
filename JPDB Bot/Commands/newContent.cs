using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPDB_Bot.Commands
{
    public class newContent : BaseCommandModule
    {
        [Command("newContent")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        [Description("See how much content is getting added in the next update")]
        public static async Task newContentCommand(CommandContext ctx)
        {
            
        }


    }
}
