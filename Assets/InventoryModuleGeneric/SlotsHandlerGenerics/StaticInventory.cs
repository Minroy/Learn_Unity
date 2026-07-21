using InventoryModule.Generics.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryModule.Generics.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class StaticInventory<T> : InventoryListModuleBase<T> where T : IItemData
    {
        private Slot<T>[] slots;
        public StaticInventory(int StartSize)
        {
            slots = new Slot<T>[StartSize];
        }

        public override bool IsFull
        {
            get => throw new NotImplementedException();
        }

        public override int Length => slots.Length;

        public override T GetValue(int index)
        {
            if (index < 0 || index >= slots.Length) throw new ArgumentOutOfRangeException();
            return slots[index].Item;
        }


        public override void GetValue(int index, out T value)
        {
            if (index < 0 || index >= slots.Length) throw new ArgumentOutOfRangeException();

            value = slots[index].Item;
        }

        // create span and dictionary
        public override int TryAdd(T item, int amountToAdd)
        {
            CheckNullOrEmpty(item, amountToAdd);

            int remaining = amountToAdd;

            // check if Item exsist in the slots and add it.
            for (int i = 0; i < slots.Length; i++)
            {
                if (remaining <= 0) break;

                if (!slots[i].IsEmpty && slots[i].Item.CanStackWith(item))
                {
                    remaining = slots[i].Add(item, remaining);
                    Debug.Log(remaining + " remaining");
                }
            }

            for (int i = 0; i < slots.Length; i++)
            {
                if (remaining <= 0) break;

            }


            return remaining;
        }

        public override int TryAdd(T item, int amountToAdd, int index)
        {
            throw new NotImplementedException();
        }


        public override int RemoveAmountAt(int index, int amountToRemove)
        {
            throw new NotImplementedException();
        }


        public override int TryRemove(T item, int amountToRemove)
        {
            throw new NotImplementedException();
        }

        public override int TryRemove(T item, int amountToRemove, int index)
        {
            throw new NotImplementedException();
        }

        public override void Clear()
        {
            throw new NotImplementedException();
        }

        public override int Clear(int index)
        {
            throw new NotImplementedException();
        }
    }
}
