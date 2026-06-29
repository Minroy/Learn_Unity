using System;
using System.Collections.Generic;
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
    private async void BindSlots(CreateContainer container)
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
    private async void UnBindSlots(CreateContainer container)
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

    public static void AddItemToContainer(CreateContainer container, ItemSO item, int amount = 1)
    {

        int remaining = container.AddToContainer(item, amount);
        if (remaining > 0)
        {
            Debug.LogWarning(container + " has " + remaining + " left over that didn't fit.");
        }
    }
    public static void RevomeItemFormContainer(CreateContainer container, ItemSO item, int amount = 1)
    {

        int remaining = container.Remove(item, amount);
        if (remaining > 0)
        {
            Debug.LogWarning(container + " has " + remaining + " left over that didn't fit.");
        }
    }

    public void Register(CreateContainer container)
    {
        BindSlots(container);
    }
    public void UnRegister(CreateContainer container)
    {
        UnBindSlots(container);
    }
}