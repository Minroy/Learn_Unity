#pragma warning disable
using InventoryModule.IDSystem;

using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Unity.Collections.LowLevel.Unsafe;

namespace InventoryModule
{
    /// <summary>
    /// A base Class that All Custom Inventory List Inherit form
    /// </summary>

    public abstract class InventoryListModuleBase : IEnumerable
    {

        protected abstract int Length { get; }
        protected abstract bool IsFull { get; }

        protected event Action<int> OnSlotsAdd;
        protected event Action<int> OnSlotsRemoved;
        protected event Action OnSlotsUpdated;


        #region Data Logic
        /// <summary>
        /// Adds the given item and amount to the Inventory.
        /// </summary>
        /// <returns> Amount that Slot couldnt add</returns>
        protected abstract int TryAdd<TAdd>(TAdd item, int amountToAdd) where TAdd : IItem;



        /// <summary>
        /// Adds the given item to inventory at index
        /// </summary>
        /// <returns> Amount that Slot couldnt add </returns>
        protected abstract int TryAdd<TAdd>(TAdd item, int amountToAdd, int index) where TAdd : IItem;



        /// <summary>
        /// Removes the given amount of item
        /// </summary>
        /// <returns> Amount that Slot couldnt removed</returns>
        protected abstract int TryRemove<TRemove>(TRemove item, int amountToRemove) where TRemove : IItem;



        /// <summary>
        /// Removes the given amount of item
        /// </summary>
        /// <returns> Amount that Slot couldnt removed</returns>
        protected abstract int TryRemove<TRemove>(TRemove item, int amountToRemove, int index) where TRemove : IItem;



        /// <summary>
        /// Removes the given amount of item at index
        /// </summary>
        /// <returns> Amount that Slot couldnt removed</returns>
        protected abstract int RemoveAmountAt(int index, int amountToRemove);

        /// <summary>
        /// Clears the entire InventoryList
        /// </summary>
        /// <returns></returns>
        protected abstract void Clear();

        /// <summary>
        /// Clears the Slot at Index, but returns amount
        /// </summary>
        protected abstract int ClearAndReturn(int index);

        /// <summary>
        /// Clears the Slot at Index
        /// </summary>
        protected abstract void Clear(int index);

        #endregion


        protected abstract T GetValue<T>(int index);

        protected abstract void GetValue<T>(int index, out T value);

        public abstract IEnumerator GetEnumerator();


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        [DoesNotReturn]
        public bool CheckNullOrEmpty<TtoCheck>(TtoCheck item, int amount) where TtoCheck : IItem
        {
            if (item == null)
            {
                return false;
            }
            else if (item.ItemId == null)
            {
                return false;
            }
            else if (amount < 0) return false;
            else return true;

        }
    }

}
