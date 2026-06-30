using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private ItemSO item;
    [SerializeField] private ItemSO item2;
    [SerializeField] private Container hotBarContainer;
    [SerializeField] private Container backPack;

    private int SpillOver;
    [SerializeField] private int amountToAdd = 1;

    private void Awake()
    {
        if (hotBarContainer != null && backPack != null)
            InventoryLogicHandler.Instance.Register(hotBarContainer, backPack); // updated register to accept params of cantainer[]
        
    }



    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (hotBarContainer.IsFull)
            {
                backPack.AddToContainer(item, SpillOver);
            }
            else
            {
                SpillOver = hotBarContainer.AddToContainer(item, amountToAdd * 10);
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
           
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

