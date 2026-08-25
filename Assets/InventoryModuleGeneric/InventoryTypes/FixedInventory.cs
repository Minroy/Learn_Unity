using InventoryModule.Data;
using InventoryModule.IDSystem;
using InventoryModule.UI;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace InventoryModule
{

    /// <summary>
    /// A Fixed inventory, that can hold anything. uses an array of premade <see cref="Slot{T}"/> by icy
    /// </summary>
    public class FixedInventory : InventoryListModuleBase
    {
        
        protected Slot<IItem>[] Slots;

        public FixedInventory(int Size)
        {
            Slots = new Slot<IItem>[Size];
        }

        protected override int Length => Slots.Length;

        //TODO:Low, Create a System that tells if its full or not
        protected override bool IsFull
        {
            get;
        }


        protected override int TryAdd<TAdd>(TAdd item, int amountToAdd)
        {
            if (!CheckNullOrEmpty(item, amountToAdd)) return amountToAdd;

            int remaining = amountToAdd;

            // Pass 1: existing slots with this item
            for (int i = 0; i < Slots.Length; i++)
            {
                if (remaining <= 0) break;

                if (!Slots[i].IsEmpty && Slots[i].Item.ItemId == item.ItemId)
                {
                    remaining = Slots[i].Add(item, remaining);
                }
            }

            // Pass 2: empty slots
            for (int i = 0; i < Slots.Length; i++)
            {
                if (remaining <= 0) break;

                if (Slots[i].IsEmpty)
                {
                    remaining = Slots[i].Add(item, remaining);
                }
            }
            
            return remaining;
        }

        protected override void Clear()
        {
            for (int i = 0; i < Slots.Length; i++)
            {
                Clear(i);
            }
        }


        //protected virtual T ClearAndReturn<T>(int index)
        //{

        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void Clear(int index)
        {
            Slots[index].Clear();
        }

        protected override int ClearAndReturn(int index)
        {
            int amount = Slots[index].Amount;
            Slots[index].Clear();
            return amount;
        }

        public override IEnumerator GetEnumerator()
        {
            return Slots.GetEnumerator();
        }

        protected override T GetValue<T>(int index)
        {
            return (T)Slots[index].GetData();
        }

        protected override void GetValue<T>(int index, out T value)
        {
            throw new System.NotImplementedException();
        }

        protected override int RemoveAmountAt(int index, int amountToRemove)
        {
            throw new System.NotImplementedException();
        }
        protected override int TryAdd<TAdd>(TAdd item, int amountToAdd, int index)
        {
            throw new System.NotImplementedException();
        }

        protected override int TryRemove<TRemove>(TRemove item, int amountToRemove)
        {
            throw new System.NotImplementedException();
        }

        protected override int TryRemove<TRemove>(TRemove item, int amountToRemove, int index)
        {
            throw new System.NotImplementedException();
        }

    }
}
