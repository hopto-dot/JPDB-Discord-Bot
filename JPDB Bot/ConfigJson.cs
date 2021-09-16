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
    }
}
