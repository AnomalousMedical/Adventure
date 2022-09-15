// This file originally comes from .net core source code
// https://github.com/dotnet/runtime
//
// The MIT License (MIT)
// 
// Copyright (c) .NET Foundation and Contributors
// 
// All rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public partial class FIRandom
    {
        /// <summary>Base type for all generator implementations that plug into the base Random.</summary>
        internal abstract class ImplBase
        {
            public abstract double Sample();

            public abstract int Next();

            public abstract int Next(int maxValue);

            public abstract int Next(int minValue, int maxValue);

            public abstract long NextInt64();

            public abstract long NextInt64(long maxValue);

            public abstract long NextInt64(long minValue, long maxValue);

            public abstract float NextSingle();

            public abstract double NextDouble();

            public abstract void NextBytes(byte[] buffer);

            public abstract void NextBytes(Span<byte> buffer);
        }
    }

    /// <summary>
    /// Represents a pseudo-random number generator, which is an algorithm that produces a sequence of numbers
    /// that meet certain statistical requirements for randomness.
    /// </summary>
    public partial class FIRandom
    {
        /// <summary>The underlying generator implementation.</summary>
        /// <remarks>
        /// This is separated out so that different generators can be used based on how this Random instance is constructed.
        /// If it's built from a seed, then we may need to ensure backwards compatibility for folks expecting consistent sequences
        /// based on that seed.  If the instance is actually derived from Random, then we need to ensure the derived type's
        /// overrides are called anywhere they were being called previously.  But if the instance is the base type and is constructed
        /// with the default constructor, we have a lot of flexibility as to how to optimize the performance and quality of the generator.
        /// </remarks>
        private readonly ImplBase _impl;

        /// <summary>Initializes a new instance of the Random class, using the specified seed value.</summary>
        /// <param name="Seed">
        /// A number used to calculate a starting value for the pseudo-random number sequence. If a negative number
        /// is specified, the absolute value of the number is used.
        /// </param>
        public FIRandom(int Seed) =>
            // With a custom seed, if this is the base Random class, we still need to respect the same algorithm that's been
            // used in the past, but we can do so without having to deal with calling the right overrides in a derived type.
            // If this is a derived type, we need to handle always using the same overrides we've done previously.
            _impl = new Net5CompatSeedImpl(Seed);

        /// <summary>Returns a non-negative random integer.</summary>
        /// <returns>A 32-bit signed integer that is greater than or equal to 0 and less than <see cref="int.MaxValue"/>.</returns>
        public virtual int Next()
        {
            int result = _impl.Next();
            AssertInRange(result, 0, int.MaxValue);
            return result;
        }

        /// <summary>Returns a non-negative random integer that is less than the specified maximum.</summary>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated. <paramref name="maxValue"/> must be greater than or equal to 0.</param>
        /// <returns>
        /// A 32-bit signed integer that is greater than or equal to 0, and less than <paramref name="maxValue"/>; that is, the range of return values ordinarily
        /// includes 0 but not <paramref name="maxValue"/>. However, if <paramref name="maxValue"/> equals 0, <paramref name="maxValue"/> is returned.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue"/> is less than 0.</exception>
        public virtual int Next(int maxValue)
        {
            if (maxValue < 0)
            {
                ThrowMaxValueMustBeNonNegative();
            }

            int result = _impl.Next(maxValue);
            AssertInRange(result, 0, maxValue);
            return result;
        }

        /// <summary>Returns a random integer that is within a specified range.</summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned. <paramref name="maxValue"/> must be greater than or equal to <paramref name="minValue"/>.</param>
        /// <returns>
        /// A 32-bit signed integer greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>; that is, the range of return values includes <paramref name="minValue"/>
        /// but not <paramref name="maxValue"/>. If minValue equals <paramref name="maxValue"/>, <paramref name="minValue"/> is returned.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="minValue"/> is greater than <paramref name="maxValue"/>.</exception>
        public virtual int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue)
            {
                ThrowMinMaxValueSwapped();
            }

            int result = _impl.Next(minValue, maxValue);
            AssertInRange(result, minValue, maxValue);
            return result;
        }

        /// <summary>Returns a non-negative random integer.</summary>
        /// <returns>A 64-bit signed integer that is greater than or equal to 0 and less than <see cref="long.MaxValue"/>.</returns>
        public virtual long NextInt64()
        {
            long result = _impl.NextInt64();
            AssertInRange(result, 0, long.MaxValue);
            return result;
        }

        /// <summary>Returns a non-negative random integer that is less than the specified maximum.</summary>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated. <paramref name="maxValue"/> must be greater than or equal to 0.</param>
        /// <returns>
        /// A 64-bit signed integer that is greater than or equal to 0, and less than <paramref name="maxValue"/>; that is, the range of return values ordinarily
        /// includes 0 but not <paramref name="maxValue"/>. However, if <paramref name="maxValue"/> equals 0, <paramref name="maxValue"/> is returned.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue"/> is less than 0.</exception>
        public virtual long NextInt64(long maxValue)
        {
            if (maxValue < 0)
            {
                ThrowMaxValueMustBeNonNegative();
            }

            long result = _impl.NextInt64(maxValue);
            AssertInRange(result, 0, maxValue);
            return result;
        }

        /// <summary>Returns a random integer that is within a specified range.</summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned. <paramref name="maxValue"/> must be greater than or equal to <paramref name="minValue"/>.</param>
        /// <returns>
        /// A 64-bit signed integer greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>; that is, the range of return values includes <paramref name="minValue"/>
        /// but not <paramref name="maxValue"/>. If minValue equals <paramref name="maxValue"/>, <paramref name="minValue"/> is returned.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="minValue"/> is greater than <paramref name="maxValue"/>.</exception>
        public virtual long NextInt64(long minValue, long maxValue)
        {
            if (minValue > maxValue)
            {
                ThrowMinMaxValueSwapped();
            }

            long result = _impl.NextInt64(minValue, maxValue);
            AssertInRange(result, minValue, maxValue);
            return result;
        }

        /// <summary>Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.</summary>
        /// <returns>A single-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
        public virtual float NextSingle()
        {
            float result = _impl.NextSingle();
            AssertInRange(result);
            return result;
        }

        /// <summary>Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.</summary>
        /// <returns>A double-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
        public virtual double NextDouble()
        {
            double result = _impl.NextDouble();
            AssertInRange(result);
            return result;
        }

        /// <summary>Fills the elements of a specified array of bytes with random numbers.</summary>
        /// <param name="buffer">The array to be filled with random numbers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        public virtual void NextBytes(byte[] buffer)
        {
            if (buffer is null)
            {
                throw new ArgumentNullException(nameof(buffer), "Buffer cannot be null.");
            }

            _impl.NextBytes(buffer);
        }

        /// <summary>Fills the elements of a specified span of bytes with random numbers.</summary>
        /// <param name="buffer">The array to be filled with random numbers.</param>
        public virtual void NextBytes(Span<byte> buffer) => _impl.NextBytes(buffer);

        /// <summary>Returns a random floating-point number between 0.0 and 1.0.</summary>
        /// <returns>A double-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
        protected virtual double Sample()
        {
            double result = _impl.Sample();
            AssertInRange(result);
            return result;
        }

        private static void ThrowMaxValueMustBeNonNegative() =>
            throw new ArgumentOutOfRangeException("maxValue", "Max value cannot be negative");

        private static void ThrowMinMaxValueSwapped() =>
            throw new ArgumentOutOfRangeException("minValue", "Min and max value swapped");

        [Conditional("DEBUG")]
        private static void AssertInRange(long result, long minInclusive, long maxExclusive)
        {
            if (maxExclusive > minInclusive)
            {
                Debug.Assert(result >= minInclusive && result < maxExclusive, $"Expected {minInclusive} <= {result} < {maxExclusive}");
            }
            else
            {
                Debug.Assert(result == minInclusive, $"Expected {minInclusive} == {result}");
            }
        }

        [Conditional("DEBUG")]
        private static void AssertInRange(double result)
        {
            Debug.Assert(result >= 0.0 && result < 1.0, $"Expected 0.0 <= {result} < 1.0");
        }
    }
    public partial class FIRandom
    {
        /// <summary>
        /// Provides an implementation used for compatibility with cases where a seed is specified
        /// and thus the sequence produced historically could have been relied upon.
        /// </summary>
        private sealed class Net5CompatSeedImpl : ImplBase
        {
            private CompatPrng _prng; // mutable struct; do not make this readonly

            public Net5CompatSeedImpl(int seed) =>
                _prng = new CompatPrng(seed);

            public override double Sample() => _prng.Sample();

            public override int Next() => _prng.InternalSample();

            public override int Next(int maxValue) => (int)(_prng.Sample() * maxValue);

            public override int Next(int minValue, int maxValue)
            {
                long range = (long)maxValue - minValue;
                return range <= int.MaxValue ?
                    (int)(_prng.Sample() * range) + minValue :
                    (int)((long)(_prng.GetSampleForLargeRange() * range) + minValue);
            }

            public override long NextInt64()
            {
                while (true)
                {
                    // Get top 63 bits to get a value in the range [0, long.MaxValue], but try again
                    // if the value is actually long.MaxValue, as the method is defined to return a value
                    // in the range [0, long.MaxValue).
                    ulong result = NextUInt64() >> 1;
                    if (result != long.MaxValue)
                    {
                        return (long)result;
                    }
                }
            }

            public override long NextInt64(long maxValue) => NextInt64(0, maxValue);

            /// <summary>Returns the integer (ceiling) log of the specified value, base 2.</summary>
            /// <param name="value">The value.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static int Log2Ceiling(ulong value)
            {
                int result = BitOperations.Log2(value);
                if (BitOperations.PopCount(value) != 1)
                {
                    result++;
                }
                return result;
            }

            public override long NextInt64(long minValue, long maxValue)
            {
                ulong exclusiveRange = (ulong)(maxValue - minValue);

                if (exclusiveRange > 1)
                {
                    // Narrow down to the smallest range [0, 2^bits] that contains maxValue - minValue
                    // Then repeatedly generate a value in that outer range until we get one within the inner range.
                    int bits = Log2Ceiling(exclusiveRange);
                    while (true)
                    {
                        ulong result = NextUInt64() >> (sizeof(long) * 8 - bits);
                        if (result < exclusiveRange)
                        {
                            return (long)result + minValue;
                        }
                    }
                }

                Debug.Assert(minValue == maxValue || minValue + 1 == maxValue);
                return minValue;
            }

            /// <summary>Produces a value in the range [0, ulong.MaxValue].</summary>
            private ulong NextUInt64() =>
                 ((ulong)(uint)Next(1 << 22)) |
                (((ulong)(uint)Next(1 << 22)) << 22) |
                (((ulong)(uint)Next(1 << 20)) << 44);

            public override double NextDouble() => _prng.Sample();

            public override float NextSingle() => (float)_prng.Sample();

            public override void NextBytes(byte[] buffer) => _prng.NextBytes(buffer);

            public override void NextBytes(Span<byte> buffer) => _prng.NextBytes(buffer);
        }

        /// <summary>
        /// Implementation used for compatibility with previous releases. The algorithm is based on a modified version
        /// of Knuth's subtractive random number generator algorithm.  See https://github.com/dotnet/runtime/issues/23198
        /// for a discussion of some of the modifications / discrepancies.
        /// </summary>
        private struct CompatPrng
        {
            private int[] _seedArray;
            private int _inext;
            private int _inextp;

            public CompatPrng(int seed)
            {
                // Initialize seed array.
                int[] seedArray = new int[56];

                int subtraction = (seed == int.MinValue) ? int.MaxValue : Math.Abs(seed);
                int mj = 161803398 - subtraction; // magic number based on Phi (golden ratio)
                seedArray[55] = mj;
                int mk = 1;

                int ii = 0;
                for (int i = 1; i < 55; i++)
                {
                    // The range [1..55] is special (Knuth) and so we're wasting the 0'th position.
                    if ((ii += 21) >= 55)
                    {
                        ii -= 55;
                    }

                    seedArray[ii] = mk;
                    mk = mj - mk;
                    if (mk < 0)
                    {
                        mk += int.MaxValue;
                    }

                    mj = seedArray[ii];
                }

                for (int k = 1; k < 5; k++)
                {
                    for (int i = 1; i < 56; i++)
                    {
                        int n = i + 30;
                        if (n >= 55)
                        {
                            n -= 55;
                        }

                        seedArray[i] -= seedArray[1 + n];
                        if (seedArray[i] < 0)
                        {
                            seedArray[i] += int.MaxValue;
                        }
                    }
                }

                _seedArray = seedArray;
                _inext = 0;
                _inextp = 21;
            }

            internal double Sample() =>
                // Including the division at the end gives us significantly improved random number distribution.
                InternalSample() * (1.0 / int.MaxValue);

            internal void NextBytes(Span<byte> buffer)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = (byte)InternalSample();
                }
            }

            internal int InternalSample()
            {
                int locINext = _inext;
                if (++locINext >= 56)
                {
                    locINext = 1;
                }

                int locINextp = _inextp;
                if (++locINextp >= 56)
                {
                    locINextp = 1;
                }

                int[] seedArray = _seedArray;
                int retVal = seedArray[locINext] - seedArray[locINextp];

                if (retVal == int.MaxValue)
                {
                    retVal--;
                }
                if (retVal < 0)
                {
                    retVal += int.MaxValue;
                }

                seedArray[locINext] = retVal;
                _inext = locINext;
                _inextp = locINextp;

                return retVal;
            }

            internal double GetSampleForLargeRange()
            {
                // The distribution of the double returned by Sample is not good enough for a large range.
                // If we use Sample for a range [int.MinValue..int.MaxValue), we will end up getting even numbers only.
                int result = InternalSample();

                // We can't use addition here: the distribution will be bad if we do that.
                if (InternalSample() % 2 == 0) // decide the sign based on second sample
                {
                    result = -result;
                }

                double d = result;
                d += int.MaxValue - 1; // get a number in range [0..2*int.MaxValue-1)
                d /= 2u * int.MaxValue - 1;
                return d;
            }
        }
    }
}
