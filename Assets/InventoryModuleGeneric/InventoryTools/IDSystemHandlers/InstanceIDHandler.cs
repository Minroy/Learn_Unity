using System;
using System.Security.Cryptography;
using UnityEngine;

namespace InventoryModule.IDSystem.Instance
{
    public static class InstanceIDHandler
    {
        public static ulong GenerateID()
        {
            Span<byte> bytes = stackalloc byte[8];
            RandomNumberGenerator.Fill(bytes);
            return BitConverter.ToUInt64(bytes);
        }

        public static ulong GetInstanceID(IInstanceable instanceID) // To replace with IItsanceItem.
        {
            return instanceID.InstanceId;
        }

        //TODO: VERYLOW
        // Make a Type the Overrides IDs, and other system will do a runtime Override
        ////public static ulong RegenerateID()
        ////{
        ////    Span<byte> bytes = stackalloc byte[8];
        ////    RandomNumberGenerator.Fill(bytes);
        ////    return BitConverter.ToUInt64(bytes);
        ////}
    }
}

namespace InventoryModule.Packer
{
    public enum CompressionMode
    {
        /// <summary>
        /// No compression.
        /// </summary>
        None,

        /// <summary>
        /// LZ4 with minimum compression effort. Fastest LZ4 option.
        /// Best for frequent runtime serialization.
        /// </summary>
        LZ4_Min,

        /// <summary>
        /// LZ4 with balanced compression and speed.
        /// </summary>
        LZ4_Medium,

        /// <summary>
        /// LZ4 with maximum compression effort.
        /// Best when compression ratio is more important than speed.
        /// </summary>
        LZ4_Max,

        /// <summary>
        /// Deflate compression.
        /// Good general-purpose compression.
        /// </summary>
        Deflate,

        /// <summary>
        /// Brotli compression.
        /// Better compression ratio, generally slower than LZ4.
        /// </summary>
        Brotli
    }
}
