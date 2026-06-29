using UnityEngine;

public class InventoryTester : MonoBehaviour
{
    [SerializeField] private ItemSO item;
    [SerializeField] private ItemSO item2;
    [SerializeField] private CreateContainer container;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("ADDING 30");
            InventoryLogicHandler.AddItemToContainer(container, item, 30);
        }


        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("REMOVING 30");
            InventoryLogicHandler.RevomeItemFormContainer(container, item, 30);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("ADDING 999");
            InventoryLogicHandler.AddItemToContainer(container, item2, 99);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Debug.Log("REMOVING 999");
            InventoryLogicHandler.RevomeItemFormContainer(container, item2, 99);
        }


        if (Input.GetMouseButtonDown(0))
        {
            PrintHoveredSlot();
        }
    }


    private void PrintHoveredSlot()
    {
        Slot slot = SlotsUI.HoveredSlot;

        if (slot == null)
        {
            Debug.Log("No slot hovered");
            return;
        }

        Debug.Log(
            slot.IsEmpty
            ? "Empty Slot"
            : $"{slot.Item.displayName} x{slot.Amount}"
        );
    }
}

