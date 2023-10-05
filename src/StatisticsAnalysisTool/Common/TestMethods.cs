using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using StatisticsAnalysisTool.Models.ItemsJsonModel;

namespace StatisticsAnalysisTool.Common;

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
        var counter = 0;
        while (true)
        {
            var itemIndex = Random.Next(20, 10000);
            var item = ItemController.GetItemByIndex(itemIndex);
            if (item.FullItemInformation is Weapon || counter > 1000)
            {
                return itemIndex;
            }
            
            counter++;
        }
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