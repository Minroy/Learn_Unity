using System.Threading.Tasks;
using UnityEngine;

[DefaultExecutionOrder(-2000)]
public class InventoryLogicHandler : MonoBehaviour
{
    public static InventoryLogicHandler Instance;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    //async to prevent lag spikes if inventorylist has 100s of times. 
    private async void BindSlots(Container container)
    {
        for (int i = 0; i < container.gameObject.transform.childCount; i++)
        {
            SlotsUI slotUI = container.gameObject.transform.GetChild(i).GetComponent<SlotsUI>();
            slotUI.Bind(container.GetSlot(i));

            if (i % 50 == 0)
            {
                await Task.Yield();
            }
        }
    }


    //async to prevent lag spikes if inventorylist has 100s of times. 
    private async void UnBindSlots(Container container)
    {
        for (int i = 0; i < container.gameObject.transform.childCount; i++)
        {
            SlotsUI slotUI = container.gameObject.transform.GetChild(i).GetComponent<SlotsUI>();
            slotUI.UnBind();

            if (i % 50 == 0)
            {
                await Task.Yield();
            }
        }
    }

    public static int AddItemToContainer(Container container, ItemSO item, int amount = 1)
    {
        int remaining = container.AddToContainer(item, amount);
        if (remaining > 0)
            Debug.LogWarning(container + " has " + remaining + " left over that didn't fit.");
        return remaining;
    }
    public static void RevomeItemFormContainer(Container container, ItemSO item, int amount = 1)
    {

        int remaining = container.RemoveForContainer(item, amount);
        if (remaining > 0)
        {
            Debug.LogWarning(container + " has " + remaining + " which couldnt be removed.");
        }
    }

    public static int AddWithOverflow(Container primary, Container overflow, ItemSO item, int amount)
    {
        int remaining = AddItemToContainer(primary, item, amount);
        if (remaining > 0)
            remaining = AddItemToContainer(overflow, item, remaining);
        return remaining;
    }

    public void Register(params Container[] container)
    {
        foreach (var containers in container)
        {
            BindSlots(containers);
        }
            
    }
    public void UnRegister(params Container[] container)
    {
        foreach (var containers in container)
        {
            UnBindSlots(containers);
        }

    }
}