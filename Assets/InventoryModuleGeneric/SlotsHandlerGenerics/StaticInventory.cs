using InventoryModule.Generics.Interfaces;
using System;
using System.Collections.Generic;

namespace InventoryModule.Generics.Data
{
    public class StaticInventory<T> : InventoryListModuleBase<T> where T : IItemData
    {
        public Slot[] slots = new Slot[0];
        public StaticInventory() { }




        public override bool IsFull
        {
            get => throw new NotImplementedException();
        }

        public override void Clear()
        {
            throw new NotImplementedException();
        }

        public override int Clear(int index)
        {
            throw new NotImplementedException();
        }

        public override T GetValue(int index)
        {
            throw new NotImplementedException();
        }

        public override void GetValue(int index, out T value)
        {
            throw new NotImplementedException();
        }

        public override int RemoveAmountAt(int index, int amountToRemove)
        {
            throw new NotImplementedException();
        }

        public override int TryAdd(T item, int amountToAdd)
        {
            throw new NotImplementedException();
        }

        public override int TryAdd(T item, int amountToAdd, int index)
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
    }
}
