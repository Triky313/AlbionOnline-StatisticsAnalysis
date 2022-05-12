using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace StatisticsAnalysisTool.Common
{
    public class TestMethods
    {
        private static readonly Random Random = new(DateTime.Now.Millisecond);

        public static string GenerateName(int len)
        {
            string[] consonants = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "l", "n", "p", "q", "r", "s", "sh", "zh", "t", "v", "w", "x" };
            string[] vowels = { "a", "e", "i", "o", "u", "ae", "y" };
            var name = "";
            name += consonants[Random.Next(consonants.Length)].ToUpper();
            name += vowels[Random.Next(vowels.Length)];
            var b = 2; //b tells how many times a new letter has been added. It's 2 right now because the first two letters are already in the name.
            while (b < len)
            {
                name += consonants[Random.Next(consonants.Length)];
                b++;
                name += vowels[Random.Next(vowels.Length)];
                b++;
            }

            return name;
        }

        public static int GetRandomWeaponIndex()
        {
            var indexArray = new List<int> { 6180, 5900, 6326, 5614, 6600, 5602, 6467, 5181, 5080, 5705, 4998, 4777, 4696, 6045, 0 };

            var index = Random.Next(indexArray.Count);
            var itemIndex = indexArray[index];
            indexArray.RemoveAt(index);
            return itemIndex;
        }

        public static void PrintProperties(object obj)
        {
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(obj))
            {
                var name = descriptor.Name;
                var value = descriptor.GetValue(obj);
                Debug.Print(@"{0}={1}", name, value);
            }
        }
    }
}