#pragma warning disable

using InventoryModule.Generics.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;

namespace InventoryModule.Generics.Data
{
    /// <summary>
    /// A base Class that All Custom Inventory List Inherit form
    /// </summary>
    public abstract class InventoryListModuleBase<T> : IEnumerable<T> where T : IItemData
    {
        public abstract int Length { get; }
        public abstract bool IsFull {  get; }

        public event Action<int> OnSlotsAdd;
        public event Action<int> OnSlotsRemoved;
        public event Action onSlotsUpdated;


        #region Data Logic
        /// <summary>
        /// Adds the given item and amount to the Inventory.
        /// </summary>
        /// <returns> Amount that Slot couldnt add</returns>
        public abstract int TryAdd(T item, int amountToAdd);



        /// <summary>
        /// Adds the given item to inventory at index
        /// </summary>
        /// <returns> Amount that Slot couldnt add </returns>
        public abstract int TryAdd(T item, int amountToAdd, int index);



        /// <summary>
        /// Removes the given amount of item
        /// </summary>
        /// <returns> Amount that Slot couldnt removed</returns>
        public abstract int TryRemove(T item, int amountToRemove);



        /// <summary>
        /// Removes the given amount of item
        /// </summary>
        /// <returns> Amount that Slot couldnt removed</returns>
        public abstract int TryRemove(T item, int amountToRemove, int index);



        /// <summary>
        /// Removes the given amount of item at index
        /// </summary>
        /// <returns> Amount that Slot couldnt removed</returns>
        public abstract int RemoveAmountAt(int index, int amountToRemove);

        /// <summary>
        /// Clears the entire InventoryList
        /// </summary>
        /// <returns></returns>
        public abstract void Clear();

        /// <summary>
        /// Clears the Slot at Index
        /// </summary>
        public abstract int Clear(int index);

        #endregion


        public abstract T GetValue(int index);
        public abstract void GetValue(int index, out T value);



        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void CheckNullOrEmpty(T item, int amount)
        {
            if (item == null) throw new ArgumentNullException($"{nameof(T)} is null");
            else if (amount < 0) throw new Exception("amount negative");
        }
    }
}
