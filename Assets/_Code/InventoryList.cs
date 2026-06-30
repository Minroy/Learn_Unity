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
   

    public int TryAdd(ItemSO item, int amount = 1)
    {
        int remaining = amount;

        // Pass 1: Stack onto existing stacks.
        foreach (var slot in Slots)
        {
            if (remaining <= 0) break;
            if (slot.Item == item && !slot.IsFull)
                remaining = slot.SlotAdd(remaining);
        }

        // Pass 2: Fill empty Slots.
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

        // Pass 3: Grow the inventory if it's dynamic.
        if (remaining > 0 && !IsFixedSize)
        {
            int slotsNeeded = Mathf.CeilToInt((float)remaining / item.MaxAmount); // check how many Slots you need
            for (int i = 0; i < slotsNeeded; i++)
            {
                if (remaining <= 0) break;
                var newSlot = new Slot();
                int toAdd = Mathf.Min(remaining, item.MaxAmount);
                newSlot.Init(item, toAdd);
                remaining -= toAdd;
                Slots.Add(newSlot);
            }
        }

        if (remaining > 0)
            Debug.Log($"{nameof(InventoryList)}: Full. {remaining} {item.displayName} couldn't be added.");

        return remaining;
    }

    public int TryRemove(ItemSO item, int amount)
    {
        int remaining = amount;

        for (int i = Slots.Count - 1; i >= 0; i--)
        {
            Slot slot = Slots[i];
            if (slot.Item != item) continue;

            int toRemove = Mathf.Min(remaining, slot.Amount);
            remaining = slot.SlotRemove(toRemove);

            if (slot.IsEmpty && !IsFixedSize && (MinSize < this.Count))
                Slots.RemoveAt(i);

            if (remaining <= 0) break;
        }

        return remaining;
    }

    public void DebugInventory()
    {
        foreach (Slot slot in Slots)
            Debug.Log(slot.IsEmpty ? "Empty" : $"{slot.Item.displayName} x{slot.Amount}");
    }

    public IEnumerator<Slot> GetEnumerator() => Slots.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}