using Newtonsoft.Json;

namespace JPDB_Bot
{
    public class ConfigJson
    {
        [JsonProperty("DiscordToken")]
        public string DiscordToken { get; private set; }
        [JsonProperty("Prefix")]
        public string Prefix { get; private set; }
        [JsonProperty("JPDBToken")]
        public string JPDBToken { get; private set; }
        [JsonProperty("LogLevel")]
        public string LogLevel { get; private set; }
        [JsonProperty("WelcomeMessages")]
        public string WelcomeMessages { get; private set; }
        [JsonProperty("MemeRatings")]
        public string MemeRatings { get; private set; }

        [JsonProperty("WelcomeChannelID")]
        public ulong WelcomeChannelID { get; private set; }

        [JsonProperty("ContributerSheetLink")]
        public string ContributerSheetLink { get; private set; }
    }
}
