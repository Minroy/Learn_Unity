using TMPro;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace InventoryModule.IDSystem
{

    [System.Serializable]
    public struct InstanceIDStruct<T>
    {
        public int InstanceID;
        public T Data; // custom data the devs, that automatically adds itself
    }

    public interface IItemInstanceID<T>
    {
    }

    /// <summary>
    /// This interfaces makes it as a item, ID is Auto-generated
    /// </summary>
    public interface IItemData
    {
        uint? ItemID { get; }
        int MaxAmount { get; }
        Sprite Icon { get; }
        public bool CanStackWith(IItemData other);
        void SetID(uint id);
    }

    public interface ISlotHandler<T> where T : IItemData
    {
        int Amount { get; set; }
        T Item { get; set; }

        bool IsEmpty { get; }
        bool IsFull { get; }
        int SpaceLeft { get; }

        int Add(T item, int amount);
        int Remove(int amount);
        void Clear();

        T GetData();
        void SetData(T data);
    }

    public interface ISlotsUIHandler
    {

        Sprite Icon { get; set; }
        TextMeshProUGUI AmountText { get; set; }

        void OnActivated(IContainerIdentifier containerIdentifier, IItemData itemData);
        void RenderSlot(IItemData itemData);
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
}