using System;
using System.Diagnostics.CodeAnalysis;

namespace CritterShell.Images
{
    // ported from https://github.com/Cyan4973/xxHash/blob/v0.6.4/xxhash.c
    // Redistribution and use in source and binary forms, with or without modification,
    // are permitted provided that the following conditions are met:

    // * Redistributions of source code must retain the above copyright notice, this
    //   list of conditions and the following disclaimer.
    // * Redistributions in binary form must reproduce the above copyright notice, this
    //   list of conditions and the following disclaimer in the documentation and/or
    //   other materials provided with the distribution.

    // THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
    // ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
    // WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
    // DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR
    // ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
    // (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
    // LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
    // ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
    // (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
    // SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
    internal class XXHash
    {
        private const UInt32 Prime32_1 = 2654435761;
        private const UInt32 Prime32_2 = 2246822519;
        private const UInt32 Prime32_3 = 3266489917;
        private const UInt32 Prime32_4 = 668265263;
        private const UInt32 Prime32_5 = 374761393;

        private const UInt64 Prime64_1 = 11400714785074694791;
        private const UInt64 Prime64_2 = 14029467366897019727;
        private const UInt64 Prime64_3 = 1609587929392839161;
        private const UInt64 Prime64_4 = 9650029242287828579;
        private const UInt64 Prime64_5 = 2870177450012600261;

        public static unsafe UInt32 Hash32(byte* input, int length)
        {
            UInt32 hash;
            UInt32 seed = 0;
            byte* end = input + length;
            if (length < 16)
            {
                hash = seed + XXHash.Prime32_5;
            }
            else
            {
                UInt32 v1 = seed + XXHash.Prime32_1 + XXHash.Prime32_2;
                UInt32 v2 = seed + XXHash.Prime32_2;
                UInt32 v3 = seed;
                UInt32 v4 = seed - XXHash.Prime32_1;

                for (byte* end16 = end - 16; input <= end16; input += sizeof(UInt32))
                {
                    v1 += *((UInt32*)input) * XXHash.Prime32_2;
                    v1 = (v1 << 13) | (v1 >> (32 - 13));
                    v1 *= XXHash.Prime32_1;

                    input += sizeof(UInt32);
                    v2 += *((UInt32*)input) * XXHash.Prime32_2;
                    v2 = (v2 << 13) | (v2 >> (32 - 13));
                    v2 *= XXHash.Prime32_1;

                    input += sizeof(UInt32);
                    v3 += *((UInt32*)input) * XXHash.Prime32_2;
                    v3 = (v3 << 13) | (v3 >> (32 - 13));
                    v3 *= XXHash.Prime32_1;

                    input += sizeof(UInt32);
                    v4 += *((UInt32*)input) * XXHash.Prime32_2;
                    v4 = (v4 << 13) | (v4 >> (32 - 13));
                    v4 *= XXHash.Prime32_1;
                }

                hash = ((v1 << 1) | (v1 >> (32 - 1))) + ((v2 << 7) | (v2 >> (32 - 7))) +
                       ((v3 << 12) | (v3 >> (32 - 12))) + ((v4 << 18) | (v4 >> (32 - 18)));
            }

            hash += (UInt32)length;

            while (input + 4 <= end)
            {
                hash += *(UInt32*)input * XXHash.Prime32_3;
                hash = ((hash << 17) | (hash >> (32 - 17))) * XXHash.Prime32_4;
                input += 4;
            }

            while (input < end)
            {
                hash += ((UInt32)(*input)) * XXHash.Prime32_5;
                hash = (hash << 11) | (hash >> (32 - 11)) * XXHash.Prime32_1;
                ++input;
            }

            hash ^= hash >> 15;
            hash *= XXHash.Prime32_2;
            hash ^= hash >> 13;
            hash *= XXHash.Prime32_3;
            hash ^= hash >> 16;

            return hash;
        }

        public static unsafe UInt64 Hash64(byte* input, int length)
        {
            UInt64 hash;
            UInt64 seed = 0;
            byte* end = input + length;
            if (length < 32)
            {
                hash = seed + XXHash.Prime64_5;
            }
            else
            {
                UInt64 v1 = seed + XXHash.Prime64_1 + XXHash.Prime64_2;
                UInt64 v2 = seed + XXHash.Prime64_2;
                UInt64 v3 = seed;
                UInt64 v4 = seed - XXHash.Prime64_1;

                for (byte* end32 = end - 32; input <= end32; input += sizeof(UInt64))
                {
                    v1 += *((UInt64*)input) * XXHash.Prime64_2;
                    v1 = (v1 << 31) | (v1 >> (64 - 31));
                    v1 *= XXHash.Prime64_1;

                    input += sizeof(UInt64);
                    v2 += *((UInt64*)input) * XXHash.Prime64_2;
                    v2 = (v2 << 31) | (v2 >> (64 - 31));
                    v2 *= XXHash.Prime64_1;

                    input += sizeof(UInt64);
                    v3 += *((UInt64*)input) * XXHash.Prime64_2;
                    v3 = (v3 << 31) | (v3 >> (64 - 31));
                    v3 *= XXHash.Prime64_1;

                    input += sizeof(UInt64);
                    v4 += *((UInt64*)input) * XXHash.Prime64_2;
                    v4 = (v4 << 31) | (v4 >> (64 - 31));
                    v4 *= XXHash.Prime64_1;
                }

                hash = ((v1 << 1) | (v1 >> (64 - 1))) + ((v2 << 7) | (v2 >> (64 - 7))) + 
                       ((v3 << 12) | (v3 >> (64 - 12))) + ((v4 << 18) | (v4 >> (64 - 18)));

                v1 = (v1 << 31) | (v1 >> (64 - 31));
                v1 *= XXHash.Prime64_1;
                hash ^= v1;
                hash = hash * XXHash.Prime64_1 + XXHash.Prime64_4;

                v2 = (v2 << 31) | (v2 >> (64 - 31));
                v2 *= XXHash.Prime64_1;
                hash ^= v2;
                hash = hash * XXHash.Prime64_1 + XXHash.Prime64_4;

                v3 = (v3 << 31) | (v3 >> (64 - 31));
                v3 *= XXHash.Prime64_1;
                hash ^= v3;
                hash = hash * XXHash.Prime64_1 + XXHash.Prime64_4;

                v4 = (v4 << 31) | (v4 >> (64 - 31));
                v4 *= XXHash.Prime64_1;
                hash ^= v4;
                hash = hash * XXHash.Prime64_1 + XXHash.Prime64_4;
            }

            hash += (UInt64)length;

            while (input + 8 <= end)
            {
                UInt64 k1 = *((UInt64*)input);
                k1 *= XXHash.Prime64_2;
                k1 = (k1 << 31) | (k1 >> (64 - 31));
                k1 *= XXHash.Prime64_1;
                hash ^= k1;
                hash = ((hash << 27) | (hash >> (64 - 27))) * XXHash.Prime64_1 + XXHash.Prime64_4;
                input += 8;
            }

            if (input + 4 <= end)
            {
                hash ^= *(UInt32*)input * XXHash.Prime64_1;
                hash = ((hash << 23) | (hash >> (64 - 23))) * XXHash.Prime64_2 + XXHash.Prime64_3;
                input += 4;
            }

            while (input < end)
            {
                hash ^= ((UInt64)(*input)) * XXHash.Prime64_5;
                hash = (hash << 11) | (hash >> (64 - 11)) * XXHash.Prime64_1;
                ++input;
            }

            hash ^= hash >> 33;
            hash *= XXHash.Prime64_2;
            hash ^= hash >> 29;
            hash *= XXHash.Prime64_3;
            hash ^= hash >> 32;

            return hash;
        }
    }
}
