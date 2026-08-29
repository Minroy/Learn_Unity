using System.Collections.Generic;

namespace InventoryModule.Data
{
    public static class InstanceGlobalRegistry
    {
        private readonly struct InstanceKey
        {
            public readonly ulong InstanceID;
            public readonly uint ItemID;

            public InstanceKey(ulong instanceID, uint itemID)
            {
                InstanceID = instanceID;
                ItemID = itemID;
            }
        }

        private struct InstanceData
        {
            public IInstanceable CurrentInstance { get; set; }
            public byte[] Data { get; set; }
        }


        private static Dictionary<InstanceKey, InstanceData> InstanceRegistry = new();

        public static void Add(IInstanceable instanceItem, byte[] bytes)
        {
            if (instanceItem is not null)
            {
                InstanceKey key = new(instanceItem.InstanceId, instanceItem.ItemId);

                InstanceData data = new()
                {
                    CurrentInstance = instanceItem,
                    Data = bytes
                };
                InstanceRegistry.Add(key, data);
            }
        }

        public static void Remove(IInstanceable instanceItem, byte[] bytes)
        {

        }

        //public static bool TryGetBytes(IInstanceable instance,out ReadOnlySpan<byte> bytes)
        //{
        //   return
        //}
    }
}

