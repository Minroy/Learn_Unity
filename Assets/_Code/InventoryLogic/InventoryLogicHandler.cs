using JetBrains.Annotations;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[DefaultExecutionOrder(-2000)]
public class InventoryLogicHandler : MonoBehaviour
{
    public static InventoryLogicHandler Instance;

    public Queue<ContainerBehaviour> WaitingForRegesting = new();

    public bool DoneRegesting;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    //async to prevent lag spikes if inventorylist has 100s of times. 
    private async void BindSlots(ContainerBehaviour container)
    {
        if (!container.IsRegistered)
        {
            Debug.Log("regesting");
            for (int i = 0; i < container.gameObject.transform.childCount; i++)
            {
                SlotsUI slotUI = container.gameObject.transform.GetChild(i).GetComponent<SlotsUI>();
                slotUI.Bind(container.GetSlot(i));

                if (i % 400 == 0)
                {
                    await Task.Yield();
                }

            }
            container.RegistingSatus(true);
        }
    }


    //async to prevent lag spikes if inventorylist has 100s of times. 
    private async void UnBindSlots(ContainerBehaviour container)
    {
        if (!container.IsRegistered)
        {
            Debug.Log("regesting");
            for (int i = 0; i < container.gameObject.transform.childCount; i++)
            {
                SlotsUI slotUI = container.gameObject.transform.GetChild(i).GetComponent<SlotsUI>();
                slotUI.UnBind();

                if (i % 400 == 0)
                {
                    await Task.Yield();
                }

            }
            container.RegistingSatus(false);
        }
    }

    [MustUseReturnValue]
    [Pure]
    public static int AddItemToContainer(ContainerBehaviour container, ItemSO item, int amount = 1)
    {
        int remaining = container.AddToContainer(item, amount);
        if (remaining > 0)
            Debug.LogWarning(container + " has " + remaining + " left over that didn't fit.");
        return remaining;
    }


    [MustUseReturnValue, Pure]
    public static int RevomeItemFormContainer(ContainerBehaviour container, ItemSO item, int amount = 1)
    {

        int remaining = container.RemoveForContainer(item, amount);
        if (remaining > 0)
        {
            Debug.LogWarning(container + " has " + remaining + " which couldnt be removed.");
        }
        return remaining;
    }

    /// <summary>
    /// This Registers the Containers with the Slots.
    /// Its Best Recommended to call this form the <see cref="ContainerBehaviour"/> itself. 
    /// </summary>
    /// <param name="container"></param>
    public void Register(params ContainerBehaviour[] container)
    {
        foreach (var containers in container)
        {
            BindSlots(containers);
        }

    }
    public void UnRegister(params ContainerBehaviour[] container)
    {
        foreach (var containers in container)
        {
            UnBindSlots(containers);
        }

    }
}