using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup),typeof(Transform))]
public sealed class CreateContainer : MonoBehaviour
{
    #region This Part is needed for the Inventory System to work.

    [SerializeField] private Transform ContainerTrans;
    [SerializeField] private InventoryList InventoryList;
    [SerializeField] private GameObject Slots;
    
    private void OnEnable()
    {
        ContainerTrans = transform;
        GenerateSlots(1);
        InventoryLogicHandler.Instance.Register(this);
    }

    /// <summary>
    /// Get the Current Slot of this Container.
    /// </summary>
    public Slot GetSlot(int index)
    {
        return InventoryList[index];
    }

    public int AddToContainer(ItemSO itemType, int amount)
    {
        int spaceleft = InventoryList.TryAdd(itemType, amount);
        return spaceleft;
    }
    public int Remove(ItemSO itemType, int amount)
    {
        int spaceleft = InventoryList.TryRemove(itemType, amount);
        return spaceleft;
    }

    private void GenerateSlots(int SlotsAmount)
    {
        InventoryList = new(true, transform.childCount);
        // slot generation code later;
    }

    //private void OnDestroy()
    //{
    //    InventoryLogicHandler.Instance.UnRegister(this);
    //}

    #endregion
}
