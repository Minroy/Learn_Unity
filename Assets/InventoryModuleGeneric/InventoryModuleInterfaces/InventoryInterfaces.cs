using TMPro;
using UnityEngine;

namespace InventoryModule.Generics.Interfaces
{

    /// <summary>
    /// This interfaces makes it as a item
    /// </summary>
    public interface IItemData
    {
        string Id { get; set; }
        int MaxAmount { get; set; }
        Sprite Icon { get; }

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