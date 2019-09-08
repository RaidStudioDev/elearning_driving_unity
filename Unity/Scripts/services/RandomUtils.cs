using System;
using System.Collections.Generic;
using System.Security.Cryptography;

public static class RandomUtils
{
    // Fisher-Yates shuffle 
    // https://en.wikipedia.org/wiki/Fisher–Yates_shuffle
    // https://stackoverflow.com/questions/273313/randomize-a-listt

    private static Random seed = new Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = seed.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static void ShuffleCrypto<T>(this IList<T> list)
    {
        RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
        int n = list.Count;
        while (n > 1)
        {
            byte[] box = new byte[1];
            do provider.GetBytes(box);
            while (!(box[0] < n * (Byte.MaxValue / n)));
            int k = (box[0] % n);
            n--;
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
