using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Base class that turns a GameObject into a Container.
/// Override methods to change behaviour.
/// </summary>
public class ContainerBehaviour : MonoBehaviour
{

    //Hook up logic. to this later, ignore events for now. 
    public event Action OnContainerOpened;
    public event Action OnContainerClosed;
    public event Action OnContainerDestroyed;
    public event Action OnContainerEnabled;
    public event Action OnContainerChanged;
    public event Action OnContainerAdded;
    public event Action OnContainerRemoved;
    public event Action OnContainerSwaped;
    public event Action OnContainerFull;

    [Header("Inventory Settings")]
    [SerializeField] protected InventoryList inventoryList;
    [SerializeField] protected GameObject SlotPrefab;
    [SerializeField] protected int StartSize = 1;
    [SerializeField] protected bool isFixedSize;


    public bool IsFull => inventoryList != null && inventoryList.IsFull;

    public bool IsRegisted { get; private set; }


    public virtual void Awake()
    {
        IsRegisted = false;

        inventoryList = GenerateList(isFixedSize, StartSize);

        InventoryLogicHandler.Instance.Register(this);
    }


    private void OnValidate()
    {
        if (SlotPrefab == null)
            return;

        if (inventoryList == null)
            inventoryList = GenerateList(isFixedSize, StartSize);

#if UNITY_EDITOR
        EditorApplication.delayCall += () =>
        {
            if (this != null)
                GenerateSlots(StartSize);
        };
#endif
    }


    public virtual InventoryList GenerateList(bool isFixedSize, int startSize)
    {
        return new InventoryList(isFixedSize, startSize);
    }


    public virtual InventoryList GenerateList(bool isFixedSize, int startSize, int maxSize)
    {
        return new InventoryList(isFixedSize, startSize, maxSize);
    }

    public virtual int AddToContainer(ItemSO itemType, int amount)
    {
        return inventoryList.TryAdd(itemType, amount);
    }

    public virtual int RemoveForContainer(ItemSO itemType, int amount)
    {
        return inventoryList.TryRemove(itemType, amount);
    }

    // ---------------- SLOT GENERATION ----------------


    [ContextMenu("Generate Slots")]
    public virtual void GenerateSlots(int amount)
    {
        RemoveSlots();
        AddSlots(amount);
    }



    public virtual void RemoveSlots()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);


#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Undo.DestroyObjectImmediate(child.gameObject);
            }
            else
#endif
            {
                Destroy(child.gameObject);
            }
        }
    }



    public virtual void AddSlots(int amount)
    {
        for (int i = 0; i < amount; i++)
        {

#if UNITY_EDITOR
            GameObject slot;
            if (!Application.isPlaying)
            {
                slot = PrefabUtility.InstantiatePrefab(SlotPrefab, transform) as GameObject;
            }
            else
            {
                slot = Instantiate(SlotPrefab, transform);
            }
#else

            GameObject slot = Instantiate(SlotPrefab, transform);

#endif


            slot.name = $"Slot_{i}";
        }
    }



    // ---------------- ACCESS ----------------


    public Slot GetSlot(int index)
    {
        return inventoryList[index];
    }


    public void RegistingSatus(bool status)
    {
        IsRegisted = status;
    }
}