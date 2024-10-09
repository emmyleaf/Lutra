using System.Collections.Generic;
using System.Linq;

namespace Lutra.Utility
{
    /// <summary>
    /// Class full of random number generation related functions.
    /// </summary>
    public static class Rand
    {

        #region Static Fields

        static readonly List<Random> randoms = [];

        #endregion

        #region Static Properties

        static Random random
        {
            get
            {
                if (randoms.Count == 0)
                {
                    randoms.Add(new Random());
                }
                return randoms[^1];
            }
        }

        /// <summary>
        /// A raw random value.
        /// </summary>
        public static float Value => (float)random.NextDouble();

        /// <summary>
        /// A random float from 0 to 360.
        /// </summary>
        public static float Angle => Float(360);

        /// <summary>
        /// Generate a random bool.
        /// </summary>
        public static bool Bool => random.Next(2) > 0;

        /// <summary>
        /// Generate a random bool.
        /// </summary>
        public static bool Flip => Bool;

        /// <summary>
        /// Generate a random sign.
        /// </summary>
        public static int Sign => Bool ? 1 : -1;

        #endregion

        #region Static Methods

        /// <summary>
        /// Push a random seed to use for all random number generation.
        /// </summary>
        /// <param name="seed">The seed.</param>
        public static void PushSeed(int seed)
        {
            randoms.Add(new Random(seed));
        }

        /// <summary>
        /// Pop the top random seed.
        /// </summary>
        /// <returns>The random seed popped.</returns>
        public static Random PopSeed()
        {
            var r = random;
            randoms.RemoveAt(randoms.Count - 1);
            return r;
        }

        /// <summary>
        /// Generate a random int.
        /// </summary>
        /// <returns>A random int.</returns>
        public static int Int()
        {
            return random.Next();
        }

        /// <summary>
        /// Generate a random int.
        /// </summary>
        /// <param name="max">Maximum value.</param>
        /// <returns>A random int.</returns>
        public static int Int(int max)
        {
            return random.Next(max);
        }

        /// <summary>
        /// Generate a random int.
        /// </summary>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <returns>A random int.</returns>
        public static int Int(int min, int max)
        {
            return random.Next(min, max);
        }

        /// <summary>
        /// Generate a random float.
        /// </summary>
        /// <returns>A random float.</returns>
        public static float Float()
        {
            return Value;
        }

        /// <summary>
        /// Generate a random float.
        /// </summary>
        /// <param name="max">The maximum value.</param>
        /// <returns>A random float.</returns>
        public static float Float(float max)
        {
            return max * Value;
        }

        /// <summary>
        /// Generate a random float.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <returns>A random float.</returns>
        public static float Float(float min, float max)
        {
            return min + (max - min) * Value;
        }

        /// <summary>
        /// Choose an element out of an array of objects.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="choices">The array of possible choices.</param>
        /// <returns>The chosen object.</returns>
        public static T Choose<T>(params T[] choices)
        {
            return choices[Int(choices.Length)];
        }

        /// <summary>
        /// Choose an element out of an array of objects.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="choices">The array of possible choices.</param>
        /// <returns>The chosen object.</returns>
        public static T ChooseElement<T>(IEnumerable<T> choices)
        {
            return choices.ElementAt(Int(choices.Count()));
        }

        /// <summary>
        /// Choose a random character out of a string.
        /// </summary>
        /// <param name="str">The string to choose from.</param>
        /// <returns>The chosen character as a string.</returns>
        public static string Choose(string str)
        {
            return str.Substring(Int(str.Length), 1);
        }

        /// <summary>
        /// Choose a random element in a collection of objects, and remove the object from the collection.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="choices">The collection of possible choices.</param>
        /// <returns>The chosen element.</returns>
        public static T ChooseRemove<T>(ICollection<T> choices)
        {
            var choice = ChooseElement(choices);
            choices.Remove(choice);
            return choice;
        }

        /// <summary>
        /// Shuffle an array of objects.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="list">The array to shuffle.</param>
        public static void Shuffle<T>(T[] list)
        {
            int i = list.Length;
            int j;
            T item;
            while (--i > 0)
            {
                item = list[i];
                list[i] = list[j = Int(i + 1)];
                list[j] = item;
            }
        }

        /// <summary>
        /// Shuffle a list of objects.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="list">The list to shuffle.</param>
        public static void Shuffle<T>(List<T> list)
        {
            int i = list.Count;
            int j;
            T item;
            while (--i > 0)
            {
                item = list[i];
                list[i] = list[j = Int(i + 1)];
                list[j] = item;
            }
        }

        /// <summary>
        /// A random percent chance from 0 to 100.
        /// </summary>
        /// <param name="percent">Percent from 0 to 100.</param>
        /// <returns>True if it succeeded.</returns>
        public static bool Chance(float percent)
        {
            return Value < percent * 0.01f;
        }

        /// <summary>
        /// Generate a random string.
        /// </summary>
        /// <param name="length">The length of the string to return.</param>
        /// <param name="charSet">The set of characters to pull from.</param>
        /// <returns>A string of randomly chosen characters.</returns>
        public static string String(int length, string charSet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")
        {
            var str = "";
            for (var i = 0; i < length; i++)
            {
                str += charSet[Int(charSet.Length)].ToString();
            }
            return str;
        }

        #endregion

    }
}
