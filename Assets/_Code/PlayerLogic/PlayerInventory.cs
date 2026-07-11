using InventoryModule;
using System;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private ItemSO item;
    [SerializeField] private ItemSO item2;
    [SerializeField] private ContainerBehaviour hotBarContainer;
    [SerializeField] private ContainerBehaviour backPack;

    private int SpillOver;
    [SerializeField] private int amountToAdd = 1;

    private void Awake()
    {
        InventoryManager.Register("RemoveAtIndex", (Action<int>)RemoveTestIndex); // here fixed
        InventoryManager.Register(nameof(Info), (Func<int, int>)Info); // here 
    }

    public void Update()
    {
        

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            int leftover = hotBarContainer.AddToContainer(item, amountToAdd * 10);
            if (leftover > 0)
            {
                backPack.AddToContainer(item, leftover);
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            int leftover = hotBarContainer.AddToContainer(item2, amountToAdd * 5);
            if (leftover > 0)
            {
                backPack.AddToContainer(item2, leftover);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log(Info(10) + " we added 10 to this slot");
        }
    }


    private void RemoveTestIndex(int amount)
    {
        InventoryManager.GetHoveredInfo(out var Cont, out var hoveredSlot, out var hoveredindex);
        Cont.RemoveAmountAtIndex(amount, hoveredindex);
    }

    private int Info(int amountToModify)
    {
        InventoryManager.GetHoveredInfo(out var _, out var hoveredSlot, out var hoveredindex);
        return hoveredSlot.Amount + amountToModify;
    }


    private void PrintHoveredSlot()
    {
        Slot slot = SlotsUI.HoveredSlot;

        if (slot == null)
        {
            Debug.Log("No slot hovered");
            return;
        }

        Debug.Log(slot.IsEmpty ? "Empty Slot" : $"{slot.Item.displayName} ,{slot.Amount}"
        );
    }
}

