using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace JPDB_Bot
{
    public class WeightedString
    {
        public string Value { get; init; }
        public int Weight { get; init; }

        public static string ChooseRandomWeightedString(Random random, WeightedString[] items)
        {
            int totalWeight = 0;
            foreach (WeightedString item in items)
            {
                totalWeight += item.Weight;
            }

            if (totalWeight > 0)
            {
                int selectedWeight = random.Next(totalWeight);
                int weightSoFar = 0;
                foreach (var item in items)
                {
                    weightSoFar += item.Weight;
                    if (weightSoFar >= selectedWeight)
                    {
                        return item.Value;
                    }
                }
            }

            // Fallback: Assign equal probability to each item
            return Utility.ChooseRandomItem(random, items).Value;
        }
    }

    public class GreetingsData : Dictionary<string, WeightedString[]>
    {
        public static GreetingsData LoadGreetings()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Loading greetings...");
            try
            {
                using FileStream fs = File.OpenRead("greetings.json");
                using StreamReader sr = new(fs, new UTF8Encoding(false));
                string json = sr.ReadToEnd();
                var data = JsonConvert.DeserializeObject<GreetingsData>(json);
                if (data == null)
                    throw new NullReferenceException();
                return data;
            }
            catch
            {
                WeightedString[] supporterGreetings =
                {
                    new WeightedString { Value = "Hi %username%様, I can speak English too ya know >:)", Weight = 1 },
                    new WeightedString { Value = "どうも、%username%様 :)", Weight = 3 },
                    new WeightedString { Value = "よおおおおおおおぉ %username%様！ :)", Weight = 1 },
                    new WeightedString { Value = "また会えて嬉しいね %username%様 :)", Weight = 2 },
                    new WeightedString { Value = "やっほおおおおおお～ %username%様 :)", Weight = 1 },
                    new WeightedString { Value = "おおおおっす! %username%様 :)", Weight = 2 },
                    new WeightedString { Value = "ハロオオオ %username%様！ :)", Weight = 2 },
                    new WeightedString { Value = "へっ！なんかあった？%username%様 :O", Weight = 1 },
                };

                WeightedString[] defaultGreetings =
                {
                    new WeightedString { Value = "どうも、%username%様 :)", Weight = 2 },
                    new WeightedString { Value = "おいお前 JPDBの支援者になれ", Weight = 1 },
                    new WeightedString { Value = "元気はないんだなあ %username%さん。JPDBを支援したら？うwう", Weight = 5 },
                    new WeightedString { Value = "おっす! %username% :)", Weight = 1 },
                    new WeightedString { Value = "ハロー %username%！ :)", Weight = 2 },
                    new WeightedString { Value = "へっ！なんかあった？%username%様 :O", Weight = 1 },
                };

                GreetingsData data = new GreetingsData
                {
                    { "Supporter", supporterGreetings },
                    { "Default", defaultGreetings }
                };

                return data;
            }
        }
    }
}
