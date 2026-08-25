using System;
using System.Collections.Generic;

namespace InventoryModule.Data
{
    public static class InstanceGlobalRegistry
    {
        private static Dictionary<ulong, byte[]> InstanceRegistry = new();

        public static void Add(IInstanceable instanceItem, byte[] bytes)
        {

        }

        public static void Remove(IInstanceable instanceItem, byte[] bytes)
        {

        }

        public static bool TryGetBytes(IInstanceable instance,out ReadOnlySpan<byte> bytes)
        {
            if (instance != null &&
                InstanceRegistry.TryGetValue(instance.InstanceId,out byte[] data))
            {
                bytes = data;
                return true;
            }

            bytes = ReadOnlySpan<byte>.Empty;
            return false;
        }
    }
}
