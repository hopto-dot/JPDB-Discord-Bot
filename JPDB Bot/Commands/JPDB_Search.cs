using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace JPDB_Bot.Commands
{
    public class JPDB_Search : BaseCommandModule
    {

        [Command("scan")]
        [Cooldown(1, 10, CooldownBucketType.Channel)]
        [Description("Search for content in the JPDB database and get statistics.\nFor example: !content steins gate")]
        public async Task scanDatabase(CommandContext ctx,
            [DescriptionAttribute("Name of the content you are searching")] [RemainingText]
            string searchString)
        {

        }

    }

}