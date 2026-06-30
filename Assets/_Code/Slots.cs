using System;
using UnityEngine;

public class Slot
{
    public ItemSO Item { get; private set; }
    public int Amount { get; private set; }

    public bool IsEmpty => Item == null;
    public int SpaceLeft => Item == null ? 0 : Item.MaxAmount - Amount;
    public bool IsFull => Item != null && Item.MaxAmount == Amount;



    // event fires for SLotsUI to notice;
    public event Action OnChanged;

    /// <summary>
    /// initialises the items <see cref="Slot"/>
    /// </summary>
    /// <param name="item"> itemSO to add</param>
    /// <param name="amount"> amount to add, defualt is 1</param>
    public void Init(ItemSO item, int amount = 1)
    {
        Item = item;
        Amount = item == null ? 0 : amount;
        OnChanged?.Invoke();
    }

    // adds amounts
    public int SlotAdd(int amountToAdd)
    {
        if (amountToAdd < 0)
        {
            throw new ArgumentOutOfRangeException("Negative numbers are not allowed in " + nameof(SlotAdd));
        }
        if (Item == null)
            throw new InvalidOperationException("Cannot add to an empty slot. Initialise it first.");

        int actuallyAdded = Mathf.Min(amountToAdd, SpaceLeft);

        Amount += actuallyAdded;

        OnChanged?.Invoke();

        return amountToAdd - actuallyAdded;
    }

    // removes amount
    public int SlotRemove(int amountToRemove)
    {
        if (amountToRemove < 0)
            throw new ArgumentOutOfRangeException("Amount to remove cannot be Negative " + nameof(SlotRemove));

        int actuallyRemoved = Mathf.Min(amountToRemove, Amount);
        Amount -= actuallyRemoved;

        if (Amount <= 0)
            Clear();
        else
            OnChanged?.Invoke();

        return amountToRemove - actuallyRemoved; // how much COULDN'T be removed
    }

    // This either stacks or swaps
    public void StackOrSwap(Slot source)
    {
        // first check if both are not null.
        // If they're the same item
        if (Item != null && source.Item != null && Item == source.Item)
        {
            // Destination is already full.
            // then swap the full one with the one that got space. 
            if (IsFull)
            {
                (ItemSO SameTemp_item, int SameTemp_amount) = (Item, Amount);
                Init(source.Item, source.Amount);
                source.Init(SameTemp_item, SameTemp_amount);
                return;
            }

            // if destination got space, then do stacking. 
            int leftOver = SlotAdd(source.Amount);
            if (leftOver == 0)
                source.Clear(); // clear if source if denstination took it all
            else
                source.SetAmount(leftOver); // just change amount. 

            return;
        }

        // Different items -> swap.
        (ItemSO item, int amount) = (Item, Amount);

        Init(source.Item, source.Amount);
        source.Init(item, amount);
    }

    public void SetAmount(int amount)
    {
        Amount = amount;

        if (Amount <= 0)
        {
            Clear();
            return;
        }

        OnChanged?.Invoke();
    }

    // clears, 
    public void Clear()
    {
        Item = null;
        Amount = 0;
        OnChanged?.Invoke();
    }
}