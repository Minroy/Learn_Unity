using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace InventoryModule
{
    [RequireComponent(typeof(GridLayoutGroup))]
    public class ContainerBehaviour : MonoBehaviour
    {

        //Hook up logic.to this later, ignore events for now.

        ////public event Action OnContainerOpened;
        ////public event Action OnContainerClosed;
        ////public event Action OnContainerDestroyed;
        ////public event Action OnContainerEnabled;
        ////public event Action OnContainerChanged;
        ////public event Action OnContainerAdded;
        ////public event Action OnContainerRemoved;
        ////public event Action OnContainerSwaped;
        ////public event Action OnContainerFull;


        [Header("Inventory Settings")]
        [SerializeField] protected InventoryList inventoryList;
        [SerializeField] protected GameObject SlotPrefab;
        [SerializeField] protected int StartSize = 1;
        [SerializeField] protected bool isFixedSize;


        public bool IsFixedSize => isFixedSize;
        public InventoryList ContainerList => inventoryList;
        public bool IsFull => inventoryList != null && inventoryList.IsFull;
        public bool IsRegistered { get; private set; }

        public virtual void Awake()
        {
            IsRegistered = false;
            inventoryList = GenerateList(isFixedSize, StartSize);
            inventoryList.OnSlotsAdd += AddSlots;

            InventoryManager.Register(this);
        }

        private void OnValidate()
        {
            if (SlotPrefab == null)
                return;

            inventoryList ??= GenerateList(isFixedSize, StartSize);
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorApplication.delayCall += () =>
                {
                    if (this != null)
                        GenerateSlots(StartSize);
                };
            }
#endif
        }

        public virtual InventoryList GenerateList(bool isFixedSize, int startSize, int maxSize = -1)
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
        public void GenerateSlots(int amount)
        {
            RemoveSlots();
            AddSlots(amount);
        }

        public void RemoveSlots()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                for (int i = transform.childCount - 1; i >= 0; i--)
                    Undo.DestroyObjectImmediate(transform.GetChild(i).gameObject);
                return;
            }
#endif

            // Runtime: Only dynamic containers can change size
            if (isFixedSize) return;

            // FIX: Safely unbind from the manager BEFORE killing the UI components
            InventoryManager.UnRegister(this);

            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);
        }

        public void AddSlots(int amount)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                for (int i = 0; i < amount; i++)
                {
                    GameObject slot = PrefabUtility.InstantiatePrefab(SlotPrefab, transform) as GameObject;
                    slot.name = $"Slot_{i}";
                    Undo.RegisterCreatedObjectUndo(slot, "Add Slot");
                }
                return;
            }
#endif

            if (isFixedSize) return;

            // 1. Bulk-spawn all the new physical layouts first
            int currentCount = transform.childCount;
            for (int i = 0; i < amount; i++)
            {
                GameObject slotObj = Instantiate(SlotPrefab, transform);
                slotObj.name = $"Slot_{currentCount + i}";
            }

            // FIX: Pull the Refresh call completely OUT of the loop!
            // Doing it inside the loop forced it to clear and re-bind everything 
            // multiple times for a single addition event (O(N^2) complexity). 
            // Now it executes exactly once after the batch instantiation is complete.
            InventoryManager.RefreshContainer(this);
        }

        public void AddAtIndex(int index)
        {
            inventoryList.AddAtIndex(index);

            GameObject slotObj = Instantiate(SlotPrefab, transform);
            slotObj.transform.SetSiblingIndex(index);
            slotObj.name = $"Slot_{index}";

            InventoryManager.RefreshContainer(this);
        }

        // ---------------- ACCESS ----------------

        public Slot GetSlot(int index)
        {
            Debug.Log(inventoryList[index]);
            return inventoryList[index];
        }

        public void RegistingSatus(bool status)
        {
            IsRegistered = status;
        }
    }
}