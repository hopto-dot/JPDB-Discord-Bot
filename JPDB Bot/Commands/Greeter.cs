using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JPDB_Bot.Commands
{
    public class Greeter : BaseCommandModule
    {
        // These will be populated by dependency injection
        public Random random;
        public GreetingsData greetingsData;

        [Command("hi")]
        [Cooldown(2, 10, CooldownBucketType.User)]
        [Description("Get a nice (or bad) response")]
        public async Task SayHi(CommandContext ctx)
        {
            Program.printCommandUse(ctx.User.Username, ctx.Message.Content);

            // Determine which set of greetings to use
            string category = "Default";
            if (ctx.Member.Roles.Any(r => r.Name is "Owner" or "Supporter" or "Server Booster"))
                category = "Supporter";

            string greeting = ChooseGreeting(category, ctx.User.Username);

            // Send the greeting
            await ctx.RespondAsync(greeting).ConfigureAwait(false);
        }

        private string ChooseGreeting(string category, string username)
        {
            string greeting;
            if (greetingsData is not null && (
                greetingsData.TryGetValue(category, out WeightedString[] greetings) ||
                greetingsData.TryGetValue("default", out greetings)))
            {
                string greetingTemplate = WeightedString.ChooseRandomWeightedString(random, greetings);
                greeting = greetingTemplate.Replace("%username%", username);
            }
            else
            {
                // Greeting of last resort
                Program.printError("!hi: Using greeting of last resort");
                greeting = "Hello, " + username;
            }

            return greeting;
        }
    }
}