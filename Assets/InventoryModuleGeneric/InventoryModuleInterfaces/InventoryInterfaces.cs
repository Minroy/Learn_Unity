using TMPro;
using UnityEngine;

namespace InventoryModule.IDSystem
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

    public interface IStackable
    {
        public void CustomStackLogic();
    }


}

namespace InventoryModule.IDSystem.Instance
{
    /// <summary>
    /// Makes the Item, a Type of Instance
    /// </summary>
    public interface IInstanceID
    {
        ulong? InstanceID { get; set; }
    }

    public interface IInstanceDataPacker
    {
        public void Write(InstanceWriter writer);
        public void Read(InstanceReader reader);
    }
}