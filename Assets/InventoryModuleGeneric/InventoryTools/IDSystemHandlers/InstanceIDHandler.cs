using System;
using System.Security.Cryptography;

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
            return instanceID.InstanceID.Value;
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
