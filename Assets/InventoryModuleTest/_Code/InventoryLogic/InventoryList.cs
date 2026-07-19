using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryModule
{
    public class InventoryList : IEnumerable<Slot>
    {
        public event Action<int> OnSlotsAdd;
        public event Action<int> OnSlotsRemoved;
        public event Action onSlotsUpdated;


        private readonly List<Slot> Slots;
        private ContainerBehaviour ContainerRef = null;

        public bool isDynamic { get; private set; }
        public int MaxSize { get; private set; }
        public int MinSize { get; private set; }
        public int Count => Slots.Count;



        public Slot this[int index] => Slots[index];

        public bool IsFull
        {
            get
            {
                foreach (var slot in Slots)
                {
                    if (!slot.IsFull)
                        return false;
                }
                return true;
            }
        }

        


        public InventoryList()
        {

        }

        public InventoryList(bool isDynamic, int startSize, int maxSize = -1)
        {
            this.isDynamic = isDynamic;
            MinSize = startSize;
            MaxSize = isDynamic ? maxSize : -1;

            Slots = new List<Slot>(startSize);
            for (int i = 0; i < startSize; i++)
                Slots.Add(new Slot());
        }

        public InventoryList(bool isDynamic, int startSize, ContainerBehaviour container, int maxSize = -1)
        {
            ContainerRef = container;
            this.isDynamic = isDynamic;
            MinSize = startSize;
            MaxSize = isDynamic ? maxSize : -1;

            Slots = new List<Slot>(startSize);
            for (int i = 0; i < startSize; i++)
                Slots.Add(new Slot());
        }


        public virtual int TryAdd(ItemSO item, int amount = 1)
        {
            int remaining = amount;

            // Pass 1: Check if there are any Item that are not maxed out. and stack on those.
            foreach (var slot in Slots)
            {
                if (remaining <= 0) break;
                if (slot.Item == item && !slot.IsFull)
                    remaining = slot.SlotAdd(remaining);
            }

            // Pass 2: Once Pass 1 done, check for emptySlots and fill them
            foreach (var slot in Slots)
            {
                if (remaining <= 0) break;
                if (slot.IsEmpty)
                {
                    int toAdd = Mathf.Min(remaining, item.MaxAmount);
                    slot.Init(item, toAdd);
                    remaining -= toAdd;
                }
            }

            // Pass 3: If there is no empty slots, and inventory is dynamic Add new slots and fill them.
            if (remaining > 0 && isDynamic)
            {
                int slotsNeeded = Mathf.CeilToInt((float)remaining / item.MaxAmount); // check how many Data you need

                remaining = DynamicAdd(item, remaining, slotsNeeded);
                OnSlotsAdd?.Invoke(slotsNeeded); // fire event to notify how many slots should be made.
            }

            if (remaining > 0 && !isDynamic)
                Debug.Log($"{nameof(InventoryList)}: Full. {remaining} {item.displayName} couldn't be added.");

            return remaining;


        }
        public void AddAtIndex(Slot slotdata, int index)
        {
            // Ensure we don't exceed max size if defined
            if (MaxSize != -1 && Slots.Count >= MaxSize) return;

            Slots.Insert(index, new Slot(slotdata.Item, slotdata.Amount));
            onSlotsUpdated?.Invoke();
        }



        private int DynamicAdd(ItemSO item, int remaining, int slotsNeeded)
        {
            // do the same logic of adding. but init and add same time. 
            for (int i = 0; i < slotsNeeded; i++)
            {
                if (remaining <= 0) break;

                var newSlot = new Slot();
                int toAdd = Mathf.Min(remaining, item.MaxAmount);
                newSlot.Init(item, toAdd);
                remaining -= toAdd;

                // Bug fixed, null ref due to event again. 
                // NOTE: Dont add Fire event before adding. It tells the wrong Index.
                Slots.Add(newSlot);
            }

            return remaining;
        }






        #region//------------------------------- Removing Logic ----------------------------------------------//

        /// <summary>
        /// This removes the Slot regardless it its dynamic or not (Aka deletes Slot)
        /// </summary>
        /// <param name="index"></param>
        public void DeleteSlot(int index)
        {
            if (index < 0 || index >= Slots.Count) return;
            OnSlotsRemoved?.Invoke(index);
            Slots.RemoveAt(index);
        }





        /// <summary>
        /// This removes the Amount at a Slot. If dynamic it will delete it
        /// </summary>
        /// <param name="amountToRemove"></param>
        /// <param name="index"></param>
        public void RemoveAmountIndex(int amountToRemove, int index)
        {
            if (index < 0 || index >= Slots.Count || amountToRemove < 0) return;

            Slot slot = Slots[index];
            int toRemove = Mathf.Min(amountToRemove, slot.Amount);
            amountToRemove = slot.SlotRemove(toRemove);


            if (Slots[index].IsEmpty && isDynamic)
                DeleteSlot(index);
        }




        /// <summary>
        /// Finds and Remove the first Slot it finds the Item. (Removes Form backwards)
        /// </summary>
        /// <returns></returns>
        public virtual int TryRemove(ItemSO item, int amount)
        {
            int remaining = amount;

            for (int i = Slots.Count - 1; i >= 0; i--)
            {
                Slot slot = Slots[i];
                if (slot.Item != item) continue;

                int toRemove = Mathf.Min(remaining, slot.Amount);
                remaining = slot.SlotRemove(toRemove);

                if (slot.IsEmpty && isDynamic && (MinSize < this.Count))
                    DeleteSlot(i);

                if (remaining <= 0) break;
            }

            return remaining;
        }
        #endregion




        public virtual void DebugInventory()
        {
            foreach (Slot slot in Slots)
                Debug.Log(slot.IsEmpty ? "Empty" : $"{slot.Item.displayName} x{slot.Amount}");
        }

        public IEnumerator<Slot> GetEnumerator() => Slots.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Runtime only. Change whater the inventory should be fixed or not.
        /// </summary>
        public void SetDynamic(bool isDynamic)
        {
            this.isDynamic = isDynamic;
        }
    }
}