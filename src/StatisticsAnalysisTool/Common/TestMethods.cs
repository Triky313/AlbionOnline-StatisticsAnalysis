using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Common
{
    public class TestMethods
    {
        private static readonly Random _random = new(DateTime.Now.Millisecond);

        public static string GenerateName(int len)
        {
            string[] consonants = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "l", "n", "p", "q", "r", "s", "sh", "zh", "t", "v", "w", "x" };
            string[] vowels = { "a", "e", "i", "o", "u", "ae", "y" };
            var Name = "";
            Name += consonants[_random.Next(consonants.Length)].ToUpper();
            Name += vowels[_random.Next(vowels.Length)];
            var b = 2; //b tells how many times a new letter has been added. It's 2 right now because the first two letters are already in the name.
            while (b < len)
            {
                Name += consonants[_random.Next(consonants.Length)];
                b++;
                Name += vowels[_random.Next(vowels.Length)];
                b++;
            }

            return Name;
        }

        public static int GetRandomWeaponIndex()
        {
            var indexArray = new List<int> { 6180, 5900, 6326, 5614, 6600, 5602, 6467, 5181, 5080, 5705, 4998, 4777, 4696, 6045, 0 };

            var index = _random.Next(indexArray.Count);
            var itemIndex = indexArray[index];
            indexArray.RemoveAt(index);
            return itemIndex;
        }
    }
}