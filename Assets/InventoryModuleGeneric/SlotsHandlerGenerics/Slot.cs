using InventoryModule.IDSystem;
using System;

namespace InventoryModule.Data
{
    /// <summary>
    /// A defualt Premade slot, Made my Icy. 
    /// </summary>
    public struct Slot<T> : ISlotHandler<T> where T : IItemData
    {
        public int Amount { get; set; }
        public T Item { get; set; }

        public readonly bool IsEmpty => Item == null;

        public readonly bool IsFull => Item != null && Item.MaxAmount == Amount;

        public readonly int SpaceLeft => Item == null ? 0 : Item.MaxAmount - Amount;

        // return what cannot be added
        public int Add(T item, int amount)
        {
            
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Cannot add negative amounts");

            // If slot is empty, initialize it with this data type
            if (IsEmpty) // states this does not exist.
            {
                Item = item;
                Amount = 0;
            }
            int toAdd = Math.Min(amount, SpaceLeft);

            Amount += toAdd;
            return amount - toAdd;
        }

        public void Clear()
        {
            Item = default(T);
            Amount = 0;
        }

        public T GetData()
        {
            return Item;
        }

        public int Remove(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (IsEmpty) return 0;

            int taken = Math.Min(amount, Amount);
            Amount -= taken;

            if (Amount <= 0) Clear();

            return taken; // Return how much we successfully removed
        }

        public void SetData(T data)
        {
            if (data == null) throw new ArgumentNullException(nameof(T));
            Item = data;
        }
    }
}
