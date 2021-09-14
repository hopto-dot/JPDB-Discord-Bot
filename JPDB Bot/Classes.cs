using System.Collections.Generic;

namespace DiscordBot
{
    public class Vocabulary
    {
        public string vocabKanji = string.Empty;
        public string vocabReading = string.Empty;
        public string[] vocabMeaning = { "" };
        public int vocabFreq = -1;
    }

    public class gamePlayer
    {
        public string username = string.Empty;
        public string jpdbUsername = string.Empty;
        public int points = 0;
        public string choice = "0";
    }


}