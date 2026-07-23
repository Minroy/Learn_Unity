using InventoryModule.Data;
using InventoryModule.IDSystem;
using System.Collections;
using UnityEngine;

namespace InventoryModule
{

    /// <summary>
    /// A Fixed inventory, that can hold anything. uses an array of premade <see cref="Slot{T}"/> by icy
    /// </summary>
    public class FixedInventory : InventoryListModuleBase
    {
        protected Slot<IItemData>[] Slots;

        public FixedInventory(int Size)
        {
            Slots = new Slot<IItemData>[Size];
        }

        public override int Length => Slots.Length;

        //TODO:Low, Create a System that tells if its full or not
        public override bool IsFull
        {
            get;
        }


        public override int TryAdd<TAdd>(TAdd item, int amountToAdd)
        {
            if (CheckNullOrEmpty(item, amountToAdd)) return amountToAdd;

            int remaining = amountToAdd;
            //Pass 1, Look for exsisting Slots containg this item
            for (int i = 0; i < Slots.Length; i++)
            {
                Slot<IItemData> slot = Slots[i];
                if (remaining <= 0)
                {
                    break;
                }

                if (slot.Item.ItemID == item.ItemID)
                {
                    remaining = slot.Add(item, remaining);
                }
            }

            //pass 2 : check for emptyslots
            foreach (var slot in Slots)
            {
                if (remaining <= 0)
                {
                    break;
                }

                if (slot.IsEmpty)
                {
                    remaining = slot.Add(item, remaining);
                }
            }

            return remaining;
        }

        public override void Clear()
        {
            throw new System.NotImplementedException();
        }

        public override int Clear(int index)
        {
            throw new System.NotImplementedException();
        }

        public override IEnumerator GetEnumerator()
        {
            return Slots.GetEnumerator();
        }

        public override T GetValue<T>(int index)
        {
            throw new System.NotImplementedException();
        }

        public override void GetValue<T>(int index, out T value)
        {
            throw new System.NotImplementedException();
        }

        public override int RemoveAmountAt(int index, int amountToRemove)
        {
            throw new System.NotImplementedException();
        }



        public override int TryAdd<TAdd>(TAdd item, int amountToAdd, int index)
        {
            throw new System.NotImplementedException();
        }

        public override int TryRemove<TRemove>(TRemove item, int amountToRemove)
        {
            throw new System.NotImplementedException();
        }

        public override int TryRemove<TRemove>(TRemove item, int amountToRemove, int index)
        {
            throw new System.NotImplementedException();
        }
    }
}
