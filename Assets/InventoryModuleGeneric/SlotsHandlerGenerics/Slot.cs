using InventoryModule.Generics.Interfaces;
using System;

namespace InventoryModule.Generics.Data
{
    /// <summary>
    /// A defualt Premade slot, Made my Icy. 
    /// </summary>
    public struct Slot<T> : ISlotHandler<T> where T : IItemData
    {
        public int Amount { get; set; }
        public T Item { get; set; }

        public bool IsEmpty => Item == null;

        public bool IsFull => Item != null && Item.MaxAmount == Amount;

        public int SpaceLeft => Item == null ? 0 : Item.MaxAmount - Amount;

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
            // If it's a completely different data, we can't stack it here. Return everything.
            else if (Item.Id != item.Id)
            {
                return amount;
            }

            // Calculate exactly what can fit using integer math
            int toAdd = Math.Min(amount, SpaceLeft);

            Amount += toAdd;

            // Return the remainder that couldn't fit
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
