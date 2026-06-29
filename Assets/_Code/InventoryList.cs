using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryList: List<Slot>
{
    public bool IsFixedSize { get; private set; }
    public int MaxSize { get; private set; }

    public InventoryList(bool isFixedSize, int startSize, int maxSize = -1) : base(startSize)
    {
        IsFixedSize = isFixedSize;
        MaxSize = isFixedSize ? startSize : maxSize;

        for (int i = 0; i < startSize; i++)
        {
            Add(new Slot());
        }
    }

    public int TryAdd(ItemSO item, int amount = 1)
    {
        int remaining = amount;

        foreach (var slot in this)
        {
            if (remaining <= 0) break;
            if (item == slot.Item && !slot.IsFull)
            {
                int toAdd = Mathf.Min(remaining, slot.SpaceLeft);
                slot.SlotAdd(toAdd);
                remaining -= toAdd;
            }
        }

        //Pass 2 : if all the current slots, of the item is full. find the a empty and add to it.
        foreach (var slot in this)
        {
            if (remaining <= 0) break;
            if (slot.IsEmpty)
            {
                int toAdd = Mathf.Min(remaining, item.MaxAmount);
                slot.Init(item, toAdd);
                remaining -= toAdd;
            }
        }

        //Pass 3 : if the List is a Dynamic Type. It Will Make New SLots on the Gameobject and Add it
        if (remaining > 0 && !IsFixedSize)
        {
            int slotsNeeded = Mathf.CeilToInt((float)remaining / item.MaxAmount);
            for (int i = 0; i < slotsNeeded; i++)
            {
                if (remaining <= 0) break;
                var newSlot = new Slot(); // see note below
                int toAdd = Mathf.Min(remaining, item.MaxAmount);
                newSlot.Init(item, toAdd);
                remaining -= toAdd;
                Add(newSlot);
            }
        }

        if (remaining > 0)
            Debug.Log(nameof(InventoryList) + ": full, " + remaining + " of " + item.displayName + " not added.");

        return remaining;
    }

    public int TryRemove(ItemSO item, int amount)
    {
        int amountToRemove = amount;
        for (int i = Count - 1; i >= 0; i--)
        {
            if (item == this[i].Item)
            {
                int currentAmount = Mathf.Min(amountToRemove, this[i].Amount);
                this[i].SlotRemove(currentAmount);
                amountToRemove -= currentAmount;

                if (this[i].IsEmpty && !IsFixedSize)
                    RemoveAt(i);
            }
        }
        return amountToRemove;
    }

    public void DebugInventory()
    {
        foreach (var slot in this)
        {
            Debug.Log(
                slot.IsEmpty
                ? "Empty"
                : slot.Item.displayName + " x" + slot.Amount
            );
        }
    }
}