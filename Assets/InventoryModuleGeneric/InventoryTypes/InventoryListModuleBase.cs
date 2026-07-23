using InventoryModule.IDSystem;
using InventoryModule.Multitasking;
using System;
using System.Collections;

namespace InventoryModule
{
    /// <summary>
    /// A base Class that All Custom Inventory List Inherit form
    /// </summary>
    public abstract class InventoryListModuleBase : TaskSystem, IEnumerable
    {
        public abstract int Length { get; }
        public abstract bool IsFull { get; }

        public event Action<int> OnSlotsAdd;
        public event Action<int> OnSlotsRemoved;
        public event Action OnSlotsUpdated;


        #region Data Logic
        /// <summary>
        /// Adds the given item and amount to the Inventory.
        /// </summary>
        /// <returns> Amount that Slot couldnt add</returns>
        public abstract int TryAdd<TAdd>(TAdd item, int amountToAdd) where TAdd : IItemData;



        /// <summary>
        /// Adds the given item to inventory at index
        /// </summary>
        /// <returns> Amount that Slot couldnt add </returns>
        public abstract int TryAdd<TAdd>(TAdd item, int amountToAdd, int index) where TAdd : IItemData;



        /// <summary>
        /// Removes the given amount of item
        /// </summary>
        /// <returns> Amount that Slot couldnt removed</returns>
        public abstract int TryRemove<TRemove>(TRemove item, int amountToRemove) where TRemove : IItemData;



        /// <summary>
        /// Removes the given amount of item
        /// </summary>
        /// <returns> Amount that Slot couldnt removed</returns>
        public abstract int TryRemove<TRemove>(TRemove item, int amountToRemove, int index) where TRemove : IItemData;



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


        public abstract T GetValue<T>(int index);

        public abstract void GetValue<T>(int index, out T value);

        public abstract IEnumerator GetEnumerator();


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool CheckNullOrEmpty<TtoCheck>(TtoCheck item, int amount) where TtoCheck : IItemData
        {
            if (item == null)
            {
                 throw new ArgumentNullException($"{nameof(TtoCheck)} is null");
            }
            else if (item.ItemID == null)
            {
                throw new ArgumentNullException($"{nameof(TtoCheck)} has no id");
            }
            else if (amount < 0) throw new Exception("amount negative");

            return true;

        }
    }
}
