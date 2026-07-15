#pragma warning disable
using InventoryModule;
using System;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField]
    private ItemSO item;
    [SerializeField]
    private ItemSO item2;
    [SerializeField]
    private ContainerBehaviour hotBarContainer;
    [SerializeField]
    private ContainerBehaviour backPack;
    private int SpillOver;
    [SerializeField]
    private int amountToAdd = 1;

    private void Start()
    {
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            var leftover = hotBarContainer.AddToContainer(item, (amountToAdd * 10));
            if ((leftover > 0))
            {
                backPack.AddToContainer(item, leftover);
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            var leftover1 = hotBarContainer.AddToContainer(item2, (amountToAdd * 5));
            if ((leftover1 > 0))
            {
                backPack.AddToContainer(item2, leftover1);
            }
        }

    }

    private void RemoveTestIndex(int amount)
    {
        
        ContainerBehaviour Cont = default(ContainerBehaviour);
        Slot hoveredSlot = default(Slot);
        int hoveredindex = default(int);
        InventoryManager.GetHoveredInfo(out Cont, out hoveredSlot, out hoveredindex);
        Cont.RemoveAmountAtIndex(1, hoveredindex);
    }
    private void RemoveTestIndex(int amount, int index)
    {
        ContainerBehaviour Cont = default(ContainerBehaviour);
        Slot hoveredSlot = default(Slot);
        int hoveredindex = default(int);
        InventoryManager.GetHoveredInfo(out Cont, out hoveredSlot, out hoveredindex);
        Cont.RemoveAmountAtIndex(1, hoveredindex);
    }

    private int Info(int amountToModify)
    {
        Slot hoveredSlot1 = default(Slot);
        int hoveredindex1 = default(int);
        InventoryManager.GetHoveredInfo(out ContainerBehaviour _, out hoveredSlot1, out hoveredindex1);
        return (hoveredSlot1.Amount + amountToModify);
    }

    private void PrintHoveredSlot()
    {
        var slot = SlotsUI.HoveredSlot;
        if ((slot == null))
        {
            Debug.Log("No slot hovered");
            return;
        }
        Debug.Log((slot.IsEmpty ? "Empty Slot" : string.Concat(slot.Item.displayName, " ,", slot.Amount)));
    }

    public void TestMethod(Action<int> newParameter)
    {
    }
}

