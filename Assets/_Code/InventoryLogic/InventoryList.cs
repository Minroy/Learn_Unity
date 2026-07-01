using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryList : IEnumerable<Slot>
{
    private readonly List<Slot> Slots;

    public bool IsFixedSize { get; private set; }
    public int MaxSize { get; private set; }
    public int MinSize { get; private set; }
    public int Count => Slots.Count;

    public event Action<int> OnSlotsAdd;
    public event Action<int> OnSlotsRemoved;

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

    public InventoryList(bool isFixedSize, int startSize, int maxSize = -1)
    {
        IsFixedSize = isFixedSize;
        MinSize = startSize;
        MaxSize = isFixedSize ? maxSize : -1;

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
        if (remaining > 0 && !IsFixedSize)
        {
            Debug.Log($"Pass 3 running. remaining: {remaining}, IsFixedSize: {IsFixedSize}, slotsNeeded: {Mathf.CeilToInt((float)remaining / item.MaxAmount)}");
            int slotsNeeded = Mathf.CeilToInt((float)remaining / item.MaxAmount); // check how many Slots you need


            // do the same logic of adding. but init and add same time. 
            for (int i = 0; i < slotsNeeded; i++)
            {
                if (remaining <= 0) break;

                var newSlot = new Slot();
                int toAdd = Mathf.Min(remaining, item.MaxAmount);
                newSlot.Init(item, toAdd);
                remaining -= toAdd;
                Debug.Log(Count);

                // Bug fixed, null ref due to event again. 
                // NOTE: Dont add Fire event before adding. It tells the wrong Index.
                Slots.Add(newSlot);
                Debug.Log($"OnSlotsAdd subscriber count: {OnSlotsAdd?.GetInvocationList().Length ?? 0}");
                OnSlotsAdd?.Invoke(Slots.Count - 1); // fire event to notify how many slots should be made.
            }
        }

        if (remaining > 0)
            Debug.Log($"{nameof(InventoryList)}: Full. {remaining} {item.displayName} couldn't be added.");

        return remaining;
    }

    public virtual int TryRemove(ItemSO item, int amount)
    {
        int remaining = amount;

        for (int i = Slots.Count - 1; i >= 0; i--)
        {
            Slot slot = Slots[i];
            if (slot.Item != item) continue;

            int toRemove = Mathf.Min(remaining, slot.Amount);
            remaining = slot.SlotRemove(toRemove);

            if (slot.IsEmpty && !IsFixedSize && (MinSize < this.Count))
            {
               // Bugged Fixed. Null event and Index being empty. 
                OnSlotsRemoved?.Invoke(i);
                Slots.RemoveAt(i);
            }

            if (remaining <= 0) break;
        }

        return remaining;
    }

    public virtual void DebugInventory()
    {
        foreach (Slot slot in Slots)
            Debug.Log(slot.IsEmpty ? "Empty" : $"{slot.Item.displayName} x{slot.Amount}");
    }

    public IEnumerator<Slot> GetEnumerator() => Slots.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}