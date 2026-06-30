using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup),typeof(Transform))]
public sealed class Container : MonoBehaviour
{
    #region This Part is needed for the Inventory System to work.

    [SerializeField] private Transform ContainerTrans;
    [SerializeField] private InventoryList InventoryList;
    [SerializeField] private GameObject Slots;



    public bool IsFull => InventoryList.IsFull;

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
    public int RemoveForContainer(ItemSO itemType, int amount)
    {
        int spaceleft = InventoryList.TryRemove(itemType, amount);
        Debug.Log(InventoryList[1]);
        return spaceleft;
    }

    private void GenerateSlots(int SlotsAmount)
    {
        InventoryList = new(true, transform.childCount);
    }

    //private void OnDestroy()
    //{
    //    InventoryLogicHandler.Instance.UnRegister(this);
    //}

    #endregion
}
