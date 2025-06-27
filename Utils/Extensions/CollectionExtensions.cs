using SwiftNPCs.Utils.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SwiftNPCs.Utils.Extensions
{
    public static class CollectionExtensions
    {
        public static T GetRandom<T>(this T[] values) => values.Length > 0 ? values[Random.Range(0, values.Length)] : default;

        public static T GetRandom<T>(this List<T> values) => values.Count > 0 ? values[Random.Range(0, values.Count)] : default;

        public static T GetRandom<T>(this T[] values, ref int lastRandom)
        {
            if (values.Length <= 0)
                return default;

            int choice = Random.Range(0, values.Length);
            if (lastRandom == choice)
                choice = values.WrapIndex(choice + (Random.Range(0, 2) == 0 ? -1 : 1));
            lastRandom = choice;
            return values[choice];
        }

        public static T GetRandom<T>(this List<T> values, ref int lastRandom)
        {
            if (values.Count <= 0)
                return default;

            int choice = Random.Range(0, values.Count);
            if (lastRandom == choice)
                choice = values.WrapIndex(choice + (Random.Range(0, 2) == 0 ? -1 : 1));
            lastRandom = choice;
            return values[choice];
        }

        public static T GetWeightedRandom<T>(this T[] values) where T : IWeight
        {
            if (values.Length <= 0)
                return default;

            int totalWeight = 0;
            foreach (T item in values)
                totalWeight += Mathf.Clamp(item.Weight, 1, int.MaxValue);

            int randomValue = Random.Range(0, totalWeight + 1);
            int cumulativeWeight = 0;

            foreach (T item in values)
            {
                cumulativeWeight += Mathf.Clamp(item.Weight, 1, int.MaxValue);
                if (randomValue <= cumulativeWeight)
                    return item;
            }

            return values[0];
        }

        public static T GetWeightedRandom<T>(this List<T> values) where T : IWeight
        {
            if (values.Count <= 0)
                return default;

            int totalWeight = 0;
            foreach (T item in values)
                totalWeight += Mathf.Clamp(item.Weight, 1, int.MaxValue);

            int randomValue = Random.Range(0, totalWeight + 1);
            int cumulativeWeight = 0;

            foreach (T item in values)
            {
                cumulativeWeight += Mathf.Clamp(item.Weight, 1, int.MaxValue);
                if (randomValue <= cumulativeWeight)
                    return item;
            }

            return values[0];
        }

        public static bool InRange(this Array values, int id) => id >= 0 && id < values.Length;
        public static bool InRange(this ICollection values, int id) => id >= 0 && id < values.Count;

        public static int WrapIndex(this Array values, int index) => index.Wrap(0, values.Length);
        public static int WrapIndex(this ICollection values, int index) => index.Wrap(0, values.Count);

        public static T GetWrap<T>(this T[] values, int index) => values[values.WrapIndex(index)];

        public static int Wrap(this int number, int min, int max) => ((number - min) % (max - min) + (max - min)) % (max - min) + min;
    }
}

