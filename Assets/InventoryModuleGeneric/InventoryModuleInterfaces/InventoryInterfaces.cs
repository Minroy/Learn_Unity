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

    public interface ISlotHandler
    {
        IItem Item { get; }
        bool IsEmpty { get; }
        bool IsFull { get; }
        int SpaceLeft { get; }
        int Amount { get; }
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
        public void WriteDataToPacker(InstanceDataWriter writer);

        /// <summary>
        /// What Data this instance will read back, During or after creation. 
        /// </summary>
        /// <param name="reader">this reads what data is given</param>
        public void ReadDataFormPacker(InstanceDataReader reader);
    }

    public interface IInstanceDataPackerAuto // todo
    {
        
    }


    public ref struct InstanceData // todo
    {

    }
}

namespace InventoryModule.Tasking
{

}