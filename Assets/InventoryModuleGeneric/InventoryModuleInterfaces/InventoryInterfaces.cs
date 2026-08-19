using InventoryModule.Packer;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace InventoryModule
{
    /// <summary>
    /// This interfaces makes this object a item, ID is Auto-generated, and will be overriden. 
    /// </summary>
    public interface IItem
    {
        uint? ItemID { get; }
        int MaxAmount { get; }
        Sprite Icon { get; }
        void SetID(uint id);
    }

    /// <summary>
    /// Identifies where a slot operation happened - which container and
    /// which slot index within it. Passed around so events/UI can react
    /// without the slot needing to know about its own container directly.
    /// </summary>
    public readonly struct SlotContext
    {
        public readonly IContainerIdentifier Container;
        public readonly int SlotIndex;

        public SlotContext(IContainerIdentifier container, int slotIndex)
        {
            Container = container;
            SlotIndex = slotIndex;
        }

        public override string ToString() => $"{Container?.GetId() ?? "?"}[{SlotIndex}]";
    }

    public interface ISlotHandler
    {
        IItem Item { get; }
        bool IsEmpty { get; }
        bool IsFull { get; }
        int SpaceLeft { get; }
        int Amount { get; }
        SlotContext Context { get; }
        void Clear();
    }

    //TODO : Create A ctx for ISlotHandler, for slotcontext
    public interface ISlotHandler<T> : ISlotHandler where T : IItem
    {
        new T Item { get; }
        int Add(T item, int amount);
        int Remove(int amount);

        T GetData();
        void SetData(T data);
    }

    public interface ISlotsUIHandler
    {

        Sprite Icon { get; set; }
        TextMeshProUGUI AmountText { get; set; }

        void OnActivated(IContainerIdentifier containerIdentifier, IItem itemData);
        void RenderSlot(IItem itemData);
        void ClearSlot();
    }

    public interface IContainerIdentifier
    {
        public string ContainerID { get; set; }

        public string GetId()
        {
            return ContainerID;
        }
    }

    public interface ISlotEvents
    {

    }

    //Custom StackLogic, shoudld this item stack or not.
    public interface IStackable
    {
        public bool CustomStackLogic();
    }


}

namespace InventoryModule
{
    /// <summary>
    /// Makes the Item, a Type of Instance
    /// </summary>
    public interface IInstanceable
    {
        public ulong? InstanceID { get; set; }
    }

    public interface IInstanceDataPacker
    {

        /// <summary>
        /// What Data this instance will have unique. Order of writing Matters.
        /// </summary>
        /// <param name="writer"> what Data that needs to be written</param>
        [ContextMenu("WERT")] //PlaceHolder
        public void WriteDataToPacker(InstanceDataWriter writer);

        /// <summary>
        /// What Data this instance will read back, During or after creation. 
        /// </summary>
        /// <param name="reader">this reads what data is given</param>
        public void ReadDataFormPacker(InstanceDataReader reader);
    }

    /// <summary>
    /// Convenience alternative to IInstanceDataPacker. Implement THIS
    /// instead when you don't want to hand-write WriteDataToPacker /
    /// ReadDataFormPacker: the packer discovers this type's public
    /// instance fields via reflection (in declaration order) and packs
    /// them automatically.
    ///
    /// Trade-off vs IInstanceDataPacker: slower (reflection) and less
    /// control (no custom ordering, no skipping fields, no versioning
    /// logic) - use IInstanceDataPacker instead for hot-path or
    /// frequently-instanced item types.
    ///
    /// Marker only - no members. The reading/writing logic that acts on
    /// this lives in InstanceDataWriter/InstanceDataReader, not here.
    /// </summary>
    public interface IInstanceDataPackerAuto
    {
    }


    /// <summary>
    /// Lightweight stack-only bundle of an item's ItemID + InstanceID,
    /// for quick lookups/comparisons without needing the full IItem /
    /// IInstanceable objects on hand.
    ///
    /// NOTE: because this is a ref struct, it CANNOT be used as a
    /// Dictionary&lt;TKey,...&gt; key or any other generic type argument
    /// (the runtime disallows ref structs there). If you need this as an
    /// actual hashmap key somewhere, it needs to be a plain readonly
    /// struct instead - flag it if that's the case.
    /// </summary>
    public readonly ref struct InstanceData
    {
        public readonly uint ItemID;
        public readonly ulong InstanceID;

        public InstanceData(uint itemID, ulong instanceID)
        {
            ItemID = itemID;
            InstanceID = instanceID;
        }

        /// <summary>
        /// Builds a lookup key from an item + instance pair. Both IDs
        /// must already be assigned - this reads existing IDs, it does
        /// not allocate new ones.
        /// </summary>
        public static InstanceData From(IItem item, IInstanceable instance)
        {
            if (item?.ItemID is not uint itemId)
                throw new InvalidOperationException("InstanceData.From: item has no assigned ItemID.");
            if (instance?.InstanceID is not ulong instanceId)
                throw new InvalidOperationException("InstanceData.From: instance has no assigned InstanceID.");

            return new InstanceData(itemId, instanceId);
        }

        public bool Equals(InstanceData other) =>
            ItemID == other.ItemID && InstanceID == other.InstanceID;

        public static bool operator ==(InstanceData left, InstanceData right) => left.Equals(right);
        public static bool operator !=(InstanceData left, InstanceData right) => !left.Equals(right);

        public override string ToString() => $"Item {ItemID} / Instance {InstanceID}";
    }
}

namespace InventoryModule.Tasking
{

}
