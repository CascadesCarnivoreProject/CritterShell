using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CritterShell.Images
{
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1407:ArithmeticExpressionsMustDeclarePrecedence", Justification = "Readability.")]
    internal class XXHash64
    {
        private const UInt64 Prime1 = 11400714785074694791UL;
        private const UInt64 Prime2 = 14029467366897019727UL;
        private const UInt64 Prime3 = 1609587929392839161UL;
        private const UInt64 Prime4 = 9650029242287828579UL;
        private const UInt64 Prime5 = 2870177450012600261UL;

        public static unsafe UInt64 Hash(byte* input, int length)
        {
            UInt64 hash;
            UInt64 seed = 0;
            byte* end = input + length;
            if (length < 32)
            {
                hash = seed + XXHash64.Prime5;
            }
            else
            {
                byte* end32 = end - 32;
                UInt64 v1 = seed + XXHash64.Prime1 + XXHash64.Prime2;
                UInt64 v2 = seed + XXHash64.Prime2;
                UInt64 v3 = seed + 0;
                UInt64 v4 = seed - XXHash64.Prime1;

                do
                {
                    v1 += *((UInt64*)input) * XXHash64.Prime2;
                    input += sizeof(UInt64);
                    v2 += *((UInt64*)input) * XXHash64.Prime2;
                    input += sizeof(UInt64);
                    v3 += *((UInt64*)input) * XXHash64.Prime2;
                    input += sizeof(UInt64);
                    v4 += *((UInt64*)input) * XXHash64.Prime2;
                    input += sizeof(UInt64);

                    v1 = Roll31(v1);
                    v2 = Roll31(v2);
                    v3 = Roll31(v3);
                    v4 = Roll31(v4);

                    v1 *= XXHash64.Prime1;
                    v2 *= XXHash64.Prime1;
                    v3 *= XXHash64.Prime1;
                    v4 *= XXHash64.Prime1;
                }
                while (input <= end32);

                hash = XXHash64.Roll1(v1) + XXHash64.Roll7(v2) + XXHash64.Roll12(v3) + XXHash64.Roll18(v4);
                v1 *= XXHash64.Prime2;
                v1 = XXHash64.Roll31(v1);
                v1 *= XXHash64.Prime1;
                hash ^= v1;
                hash = hash * XXHash64.Prime1 + XXHash64.Prime4;

                v2 *= XXHash64.Prime2;
                v2 = XXHash64.Roll31(v2);
                v2 *= XXHash64.Prime1;
                hash ^= v2;
                hash = hash * XXHash64.Prime1 + XXHash64.Prime4;

                v3 *= XXHash64.Prime2;
                v3 = XXHash64.Roll31(v3);
                v3 *= XXHash64.Prime1;

                hash ^= v3;
                hash = hash * XXHash64.Prime1 + XXHash64.Prime4;

                v4 *= XXHash64.Prime2;
                v4 = XXHash64.Roll31(v4);
                v4 *= XXHash64.Prime1;
                hash ^= v4;
                hash = hash * XXHash64.Prime1 + XXHash64.Prime4;
            }

            hash += (UInt64)length;

            while (input + 8 <= end)
            {
                UInt64 k1 = *((UInt64*)input);
                k1 *= XXHash64.Prime2;
                k1 = XXHash64.Roll31(k1);
                k1 *= XXHash64.Prime1;
                hash ^= k1;
                hash = XXHash64.Roll27(hash) * XXHash64.Prime1 + XXHash64.Prime4;
                input += 8;
            }

            if (input + 4 <= end)
            {
                hash ^= *(UInt32*)input * XXHash64.Prime1;
                hash = XXHash64.Roll23(hash) * XXHash64.Prime2 + XXHash64.Prime3;
                input += 4;
            }

            while (input < end)
            {
                hash ^= ((UInt64)(*input)) * XXHash64.Prime5;
                hash = XXHash64.Roll11(hash) * XXHash64.Prime1;
                ++input;
            }

            hash ^= hash >> 33;
            hash *= XXHash64.Prime2;
            hash ^= hash >> 29;
            hash *= XXHash64.Prime3;
            hash ^= hash >> 32;

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt64 Roll1(UInt64 v)
        {
            return (v << 1) | (v >> (64 - 1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt64 Roll7(UInt64 v)
        {
            return (v << 7) | (v >> (64 - 7));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt64 Roll11(UInt64 v)
        {
            return (v << 11) | (v >> (64 - 11));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt64 Roll12(UInt64 v)
        {
            return (v << 12) | (v >> (64 - 12));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt64 Roll18(UInt64 v)
        {
            return (v << 18) | (v >> (64 - 18));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt64 Roll23(UInt64 v)
        {
            return (v << 23) | (v >> (64 - 23));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt64 Roll27(UInt64 v)
        {
            return (v << 27) | (v >> (64 - 27));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt64 Roll31(UInt64 v)
        {
            return (v << 31) | (v >> (64 - 31));
        }
    }
}
