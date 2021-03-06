using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;

namespace Engine
{
    public class NaturalSortAlgorithm
    {
        public static int CompareFunc(String s1, String s2)
        {
            try
            {
                if (s1 == null)
                {
                    return 0;
                }
                if (s2 == null)
                {
                    return 0;
                }

                int len1 = s1.Length;
                int len2 = s2.Length;
                int marker1 = 0;
                int marker2 = 0;

                // Walk through two the strings with two markers.
                while (marker1 < len1 && marker2 < len2)
                {
                    char ch1 = s1[marker1];
                    char ch2 = s2[marker2];

                    // Some buffers we can build up characters in for each chunk.
                    char[] space1 = new char[len1];
                    int loc1 = 0;
                    char[] space2 = new char[len2];
                    int loc2 = 0;

                    // Walk through all following characters that are digits or
                    // characters in BOTH strings starting at the appropriate marker.
                    // Collect char arrays.
                    do
                    {
                        space1[loc1++] = ch1;
                        marker1++;

                        if (marker1 < len1)
                        {
                            ch1 = s1[marker1];
                        }
                        else
                        {
                            break;
                        }
                    } while (char.IsDigit(ch1) == char.IsDigit(space1[0]));

                    do
                    {
                        space2[loc2++] = ch2;
                        marker2++;

                        if (marker2 < len2)
                        {
                            ch2 = s2[marker2];
                        }
                        else
                        {
                            break;
                        }
                    } while (char.IsDigit(ch2) == char.IsDigit(space2[0]));

                    // If we have collected numbers, compare them numerically.
                    // Otherwise, if we have strings, compare them alphabetically.
                    string str1 = new string(space1);
                    string str2 = new string(space2);

                    int result;

                    if (char.IsDigit(space1[0]) && char.IsDigit(space2[0]))
                    {
                        int thisNumericChunk = NumberParser.ParseInt(str1);
                        int thatNumericChunk = NumberParser.ParseInt(str2);
                        result = thisNumericChunk.CompareTo(thatNumericChunk);
                    }
                    else
                    {
                        result = str1.CompareTo(str2);
                    }

                    if (result != 0)
                    {
                        return result;
                    }
                }
                return len1 - len2;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }

    /// <summary>
    /// This class provides a function that can do natural sorting on a pair of strings.
    /// </summary>
    public class NaturalSort<T> : Comparer<T>
    {
        /// <summary>
        /// Compare s1 to s2. Returns output that will work for the Comparer methods on collections.
        /// </summary>
        /// <param name="x">The first object.</param>
        /// <param name="y">The second object.</param>
        /// <returns>&lt; 0 if s1 &lt; 0, 0 if s1 == s2 and &gt; 0 if s1 &gt; s2.</returns>
        public override int Compare(T x, T y)
        {
            return NaturalSortAlgorithm.CompareFunc(x.ToString(), y.ToString());
        }
    }
}
