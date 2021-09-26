using System;

namespace JPDB_Bot.FreqGame
{
    internal class Player
    {
        public string Username { get; init; }
        public string JpdbUsername { get; set; }
        public int Points { get; set; } = 0;

        public Player(string username, string jpdbUsername)
        {
            Username = username;
            JpdbUsername = jpdbUsername;
        }
    }

    public class Vocabulary
    {
        public string vocabKanji = string.Empty;
        public string vocabReading = string.Empty;
        public string[] vocabMeaning = { "" };
        public int vocabFreq = -1;
    }
    internal class Question
    {
        private readonly Vocabulary[] Words;
        private readonly int CorrectIndex;

        public Question(Vocabulary word1, Vocabulary word2, Random rng)
        {
            int randomInt = rng.Next(2);
            var words = new[] { word1, word2 };
            Words = new[] { words[randomInt], words[1 - randomInt] };
            CorrectIndex = Words[0].vocabFreq < Words[1].vocabFreq ? 0 : 1;
        }

        public Vocabulary WordA => Words[0];
        public Vocabulary WordB => Words[1];
        public Vocabulary CorrectWord => Words[CorrectIndex];
        public Vocabulary WrongWord => Words[1 - CorrectIndex];
    }

    public class TimeoutException : Exception
    {
        public TimeoutException()
        {
        }

        public TimeoutException(string message) : base(message)
        {
        }

        public TimeoutException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public class GameCancelledException : Exception
    {
        public GameCancelledException()
        {
        }

        public GameCancelledException(string message) : base(message)
        {
        }

        public GameCancelledException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}