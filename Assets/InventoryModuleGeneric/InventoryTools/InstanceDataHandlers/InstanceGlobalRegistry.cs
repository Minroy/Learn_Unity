using System;
using System.Collections.Concurrent;
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
            public IInstanceable CurrentInstance;
            public byte[] Data;
        }

        private static ConcurrentDictionary<InstanceKey, InstanceData> InstanceRegistry = new();

        public static void Add(IInstanceable instanceItem,byte[] bytes)
        {
            if (instanceItem is null) return;

            InstanceKey key = new(instanceItem.InstanceId, instanceItem.ItemId);
            InstanceData data = new() { CurrentInstance = instanceItem, Data = bytes};

            InstanceRegistry.TryAdd(key, data);
        }

        public static bool TryGet(IInstanceable instanceItem, out ReadOnlySpan<byte> bytes)
        {
            InstanceKey key = new(instanceItem.InstanceId, instanceItem.ItemId);

            if (InstanceRegistry.TryGetValue(key, out var data))
            {
                bytes = data.Data.AsSpan();
                return true;
            }

            bytes = default;
            return false;
        }

        public static void Remove(IInstanceable instanceItem)
        {
            InstanceKey key = new(instanceItem.InstanceId, instanceItem.ItemId);
            InstanceRegistry.TryRemove(key, out _);
        }
    }
}